using CareEMR.Api.Auth;
using CareEMR.Api.OpenApi;
using CareEMR.Application.Patients;
using CareEMR.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ---------- Services ----------

builder.Services.AddControllers();

var connectionString = builder.Configuration.GetConnectionString("CareEmr")
    ?? throw new InvalidOperationException("Connection string 'CareEmr' not found.");

builder.Services.AddInfrastructure(connectionString);
builder.Services.AddScoped<PatientService>();

// JWT settings: Issuer/Audience from appsettings.json, SigningKey from User Secrets / env vars.
var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
if (jwt.SigningKey.Length < 32)
    throw new InvalidOperationException(
        "Jwt:SigningKey is missing or shorter than 32 characters. Set it in User Secrets (or the .env file for Docker).");

var demoUser = builder.Configuration.GetSection(DemoUserOptions.SectionName).Get<DemoUserOptions>() ?? new DemoUserOptions();
if (string.IsNullOrWhiteSpace(demoUser.Username) || string.IsNullOrWhiteSpace(demoUser.Password))
    throw new InvalidOperationException(
        "DemoUser:Username and DemoUser:Password must be set in User Secrets (or the .env file for Docker).");

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<DemoUserOptions>(builder.Configuration.GetSection(DemoUserOptions.SectionName));
builder.Services.AddSingleton<TokenService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false; // keep claim names as-is ("sub", not a long URL)
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = TokenService.CreateSigningKey(jwt.SigningKey),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });
builder.Services.AddAuthorization();

// OpenAPI document (+ Bearer token support so Scalar can send the token).
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecurityTransformer>();
    options.AddOperationTransformer<BearerSecurityTransformer>();
});

var app = builder.Build();

// Off by default; docker-compose turns it on so the container creates its own tables.
if (app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
    await app.Services.ApplyMigrationsAsync();

// ---------- HTTP pipeline ----------

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();                    // OpenAPI JSON at /openapi/v1.json
    app.MapScalarApiReference(options => // Interactive docs at /scalar
    {
        options.Title = "CareEMR Patient Registry API";
        options.AddPreferredSecuritySchemes(BearerSecurityTransformer.SchemeName);
    });
}

app.UseHttpsRedirection();

app.UseAuthentication(); // who are you? (reads the Bearer token)
app.UseAuthorization();  // are you allowed? (checks [Authorize])

app.MapControllers();

app.Run();

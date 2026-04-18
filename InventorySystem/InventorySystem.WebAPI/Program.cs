using InventorySystem.Application.DependencyInjection;
using InventorySystem.Application.Interfaces;
using InventorySystem.Infrastructure.DependencyInjection;
using InventorySystem.Infrastructure.Data;
using InventorySystem.Infrastructure.Identity;
using InventorySystem.WebAPI.Middleware;
using InventorySystem.WebAPI.Options;
using InventorySystem.WebAPI.Services;
using InventorySystem.WebAPI.Services.Integrations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Net;
using System.Text.Json;
using System.Text;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient();
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<SecurityHardeningOptions>(builder.Configuration.GetSection("Security"));
builder.Services.Configure<BackupOptions>(builder.Configuration.GetSection("Backup"));
builder.Services.Configure<IntegrationFeatureFlagsOptions>(builder.Configuration.GetSection("Integrations:FeatureFlags"));

builder.Services.AddScoped<ISystemBackupService, SqlServerBackupService>();
builder.Services.AddScoped<ICommercialService, EfCommercialService>();
builder.Services.AddScoped<ITallyIntegrationConnector, TallyIntegrationConnector>();
builder.Services.AddScoped<IGspGstConnector, GspGstConnector>();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

var redisConn = builder.Configuration["Redis:ConnectionString"];
if (!string.IsNullOrWhiteSpace(redisConn))
{
    builder.Services.AddStackExchangeRedisCache(o => o.Configuration = redisConn);
}
else
{
    builder.Services.AddDistributedMemoryCache();
}

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Inventory Management System API",
        Version = "v1",
        Description = "Enterprise Inventory Management System - .NET 6 + Clean Architecture"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {your_token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();

var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSettings["Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
{
    jwtKey = Environment.GetEnvironmentVariable("JWT__KEY")
        ?? "super_secret_key_that_is_long_enough_for_hmac_sha256";
}

var key = Encoding.ASCII.GetBytes(jwtKey);
var issuer = jwtSettings["Issuer"];
var audience = jwtSettings["Audience"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = !string.IsNullOrWhiteSpace(issuer),
        ValidIssuer = issuer,
        ValidateAudience = !string.IsNullOrWhiteSpace(audience),
        ValidAudience = audience,
        ClockSkew = TimeSpan.FromMinutes(2)
    };
});

var securityOptions = builder.Configuration.GetSection("Security").Get<SecurityHardeningOptions>() ?? new SecurityHardeningOptions();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanApproveAdjustments", policy => policy.RequireRole("SuperAdmin", "InventoryManager"));
    options.AddPolicy("CanViewCosts", policy => policy.RequireRole("SuperAdmin", "InventoryManager", "WarehouseStaff"));

    if (securityOptions.RequireAuthenticatedByDefault)
    {
        options.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
    }
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("ConfiguredOrigins", policy =>
    {
        var origins = securityOptions.AllowedOrigins ?? Array.Empty<string>();
        if (origins.Length == 0)
        {
            policy.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod();
        }
        else
        {
            policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
        }
    });
});

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/json";

        var feature = context.Features.Get<IExceptionHandlerFeature>();
        var payload = new
        {
            title = "Internal Server Error",
            message = "Unexpected server error. Please contact support if issue persists.",
            traceId = context.TraceIdentifier,
            detail = app.Environment.IsDevelopment() ? feature?.Error?.Message : null
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    });
});

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    if (app.Environment.IsEnvironment("Testing"))
    {
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
    }
    else
    {
        await context.Database.MigrateAsync();
    }

    await DemoDataSeeder.SeedAsync(context, userManager, roleManager);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseResponseCompression();
app.UseCors("ConfiguredOrigins");
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<SimpleRateLimitMiddleware>();
app.UseMiddleware<IdempotencyMiddleware>();

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<CommercialSubscriptionMiddleware>();

app.MapHealthChecks("/health").AllowAnonymous();
app.MapControllers();

app.Run();

public partial class Program { }

using Azure.Identity;
using Duende.AspNetCore.Authentication.OAuth2Introspection;
using home_health_be.Config;
using home_health_be.Data;
using home_health_be.Services;
using home_health_be.Services.Auth;
using home_health_be.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

#region Authentication & Authorization
var oktaEnabled = builder.Configuration.GetValue<bool>("Authentication:OktaEnabled");

if (oktaEnabled)
{
    builder.Services.Configure<OktaConfig>(builder.Configuration.GetSection("Okta"));
    var oktaConfig = builder.Configuration.GetSection("Okta").Get<OktaConfig>();
    var oktaService = new OktaService(oktaConfig);
    var discoveryDocument = await ((IOktaService)oktaService).GetOpenIdConnectConfigurationAsync();
    var introspectionEndpoint = discoveryDocument?.IntrospectionEndpoint;
    var userInfoEndpoint = discoveryDocument?.UserInfoEndpoint;

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = OAuth2IntrospectionDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OAuth2IntrospectionDefaults.AuthenticationScheme;
    })
        .AddOAuth2Introspection(option =>
        {
            option.ClientId = oktaConfig?.ClientId;
            option.IntrospectionEndpoint = introspectionEndpoint;
            option.SkipTokensWithDots = false;
            option.TokenTypeHint = "access_token";
            option.SaveToken = true;

            option.Events = new OAuth2IntrospectionEvents
            {
                OnTokenValidated = async context =>
                {
                    var accessToken = context.SecurityToken;
                    if (string.IsNullOrEmpty(userInfoEndpoint) || string.IsNullOrEmpty(accessToken))
                    {
                        context.Fail("Invalid token or user info endpoint.");
                        return;
                    }

                    var userInfo = await ((IOktaService)oktaService).GetUserInfoAsync(userInfoEndpoint, accessToken);

                    if (userInfo == null)
                    {
                        context.Fail("Failed to retrieve user info.");
                        return;
                    }

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Email, userInfo.Email),
                        new Claim(ClaimTypes.Name, userInfo.Name)
                    };

                    if (!string.IsNullOrEmpty(userInfo.PreferredName))
                    {
                        var atIndex = userInfo.PreferredName.IndexOf('@');
                        if (atIndex > 0)
                        {
                            var oktaId = userInfo.PreferredName.Substring(0, atIndex);
                            claims.Add(new Claim("okta_id", oktaId));
                        }
                    }

                    var graphClient = context.HttpContext.RequestServices.GetRequiredService<GraphServiceClient>();
                    var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
                    var msGraph = new MSGraph(graphClient, configuration);

                    var roles = await msGraph.GetUserRolesAsync(userInfo);
                    foreach (var role in roles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }

                    var appIdentity = new ClaimsIdentity(claims, context.Scheme.Name);
                    context.Principal = new ClaimsPrincipal(appIdentity);
                    context.Success();
                }
            };
        });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("EDExternalPolicy", policy =>
        {
            policy.AddAuthenticationSchemes(OAuth2IntrospectionDefaults.AuthenticationScheme);
            policy.RequireAuthenticatedUser();
        });

        options.AddPolicy("UserPolicy", policy =>
        {
            policy.AddAuthenticationSchemes(OAuth2IntrospectionDefaults.AuthenticationScheme);
            policy.RequireRole("User", "Leader");
        });

        options.AddPolicy("ApproverPolicy", policy =>
        {
            policy.AddAuthenticationSchemes(OAuth2IntrospectionDefaults.AuthenticationScheme);
            policy.RequireRole("Approver", "Leader");
        });

        options.AddPolicy("ReportPolicy", policy =>
        {
            policy.AddAuthenticationSchemes(OAuth2IntrospectionDefaults.AuthenticationScheme);
            policy.RequireRole("Report", "Leader");
        });

        options.AddPolicy("LeaderPolicy", policy =>
        {
            policy.AddAuthenticationSchemes(OAuth2IntrospectionDefaults.AuthenticationScheme);
            policy.RequireRole("Leader");
        });

        options.AddPolicy("UserOrApproverPolicy", policy =>
        {
            policy.AddAuthenticationSchemes(OAuth2IntrospectionDefaults.AuthenticationScheme);
            policy.RequireRole("User", "Approver", "Leader");
        });
    });
}
else
{
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("EDExternalPolicy", policy => policy.RequireAssertion(_ => true));
        options.AddPolicy("UserPolicy", policy => policy.RequireAssertion(_ => true));
        options.AddPolicy("ApproverPolicy", policy => policy.RequireAssertion(_ => true));
        options.AddPolicy("ReportPolicy", policy => policy.RequireAssertion(_ => true));
        options.AddPolicy("LeaderPolicy", policy => policy.RequireAssertion(_ => true));
        options.AddPolicy("UserOrApproverPolicy", policy => policy.RequireAssertion(_ => true));
    });
}
#endregion

#region CORS
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials()
                  .SetPreflightMaxAge(TimeSpan.FromSeconds(2520));
        });
});
#endregion

#region Database
builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
#endregion

#region Third-Party Services
// Microsoft Graph API configuration
builder.Services.Configure<GraphClientConfig>(
    builder.Configuration.GetSection("MicrosoftGraph"));

builder.Services.AddSingleton<GraphServiceClient>(sp =>
{
    var config = sp.GetRequiredService<IOptions<GraphClientConfig>>().Value;

    var credential = new ClientSecretCredential(
        config.TenantId,
        config.ClientId,
        config.ClientSecret);

    return new GraphServiceClient(
        credential,
        ["https://graph.microsoft.com/.default"]);
});


builder.Services.AddScoped<MSGraph>();

// SharePoint configuration
builder.Services.Configure<SharePointConfig>(
    builder.Configuration.GetSection("SharePoint"));

builder.Services.AddScoped<SharePointAttachments>();
#endregion

#region Services
builder.Services.AddScoped<IUserService, UserService>();

// Register IHttpContextAccessor to enable access to HttpContext in services
builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
#endregion

#region Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "N95 Questionnaire API",
        Description = "An ASP.NET Core Web API for managing N95 Questionnaire evaluations.",
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
#endregion

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("AllowSpecificOrigin");

app.UseAuthentication();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();

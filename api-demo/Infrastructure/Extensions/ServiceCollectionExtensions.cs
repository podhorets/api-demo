using System.Text;
using api_demo.Common.Interfaces;
using api_demo.Common.Models;
using api_demo.Common.Options;
using api_demo.Features.Auth.Login;
using api_demo.Features.Auth.Logout;
using api_demo.Features.Auth.PasswordReset.ConfirmPasswordReset;
using api_demo.Features.Auth.PasswordReset.RequestPasswordReset;
using api_demo.Features.Auth.RefreshToken;
using api_demo.Features.Auth.Signup;
using api_demo.Features.Baskets.AddItem;
using api_demo.Features.Baskets.Create;
using api_demo.Features.Baskets.Delete;
using api_demo.Features.Baskets.GetById;
using api_demo.Features.Baskets.RemoveItem;
using api_demo.Features.Baskets.Search;
using api_demo.Features.Baskets.Update;
using api_demo.Features.Baskets.UpdateItem;
using api_demo.Infrastructure.Persistence;
using api_demo.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

namespace api_demo.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(opts =>
            opts.UseNpgsql(config.GetConnectionString("DefaultConnection")));

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenService, TokenService>();

        return services;
    }

    public static IServiceCollection AddAuth(
        this IServiceCollection services, IConfiguration config)
    {
        services.AddOptions<JwtOptions>()
            .Bind(config.GetSection("Jwt"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        var jwtOptions = config.GetRequiredSection("Jwt").Get<JwtOptions>()!;
        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opts =>
            {
                opts.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(jwtOptions.Secret)),
                    ClockSkew = TimeSpan.Zero
                };

                opts.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";
                        return context.Response.WriteAsJsonAsync(
                            new ApiErrorResponse("UNAUTHORIZED", "Authentication required."));
                    }
                };
            });

        services.AddAuthorization();
        return services;
    }

    public static IServiceCollection AddFeatureHandlers(this IServiceCollection services)
    {
        services.AddScoped<CreateBasketHandler>();
        services.AddScoped<GetBasketHandler>();
        services.AddScoped<SearchBasketsHandler>();
        services.AddScoped<UpdateBasketHandler>();
        services.AddScoped<DeleteBasketHandler>();
        services.AddScoped<AddItemHandler>();
        services.AddScoped<UpdateItemHandler>();
        services.AddScoped<RemoveItemHandler>();
        
        services.AddScoped<SignupHandler>();
        services.AddScoped<LoginHandler>();
        services.AddScoped<LogoutHandler>();
        services.AddScoped<RefreshTokenHandler>();
        services.AddScoped<RequestPasswordResetHandler>();
        services.AddScoped<ConfirmPasswordResetHandler>();

        return services;
    }
    
    public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "api-demo API", Version = "v1",
                Description = "Shopping Basket API with JWT Authentication"
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "Enter 'Bearer' followed by your JWT token",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });
            
            c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });
        });
        return services;
    }
}
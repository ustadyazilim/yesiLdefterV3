/* Core Namespace */
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
/* JWT Namespace */
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
/* HTTP Namespace */
using System.Text;
using Microsoft.OpenApi.Models;
/* Ustad Namespace */
using Ustad.API.Classes;

namespace Ustad.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // NOTE(@Janberk): Enable CORS
            services.AddCors(c =>
            {
                c.AddPolicy("AllowOrigin", options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            });
            services.AddControllersWithViews().AddNewtonsoftJson(options =>
            options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore)
                .AddNewtonsoftJson(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver());
            services.AddControllers();
            // NOTE(@Janberk): Swagger/OpenAPI configuration
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Ustad API",
                    Version = "v1.6.2",
                    Description = "Authentication and tenant management API for Ustad Web Platform v1.6.2",
                    Contact = new OpenApiContact
                    {
                        Name = "Ustad Development Team"
                    }
                });
                // NOTE(@Janberk): Include XML comments for better documentation
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
                }
                else
                {
                    var altXmlPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "bin", "Debug", "net8.0", xmlFile);
                    if (File.Exists(altXmlPath))
                    {
                        c.IncludeXmlComments(altXmlPath, includeControllerXmlComments: true);
                    }
                }
                // NOTE(@Janberk): Add JWT Bearer authentication definition
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
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
            services.AddSingleton<EmailService>();
            var jwtKeyRaw = Environment.GetEnvironmentVariable("JWT_KEY") ?? Configuration["Jwt:Key"];            
            if (string.IsNullOrWhiteSpace(jwtKeyRaw))
            {
                throw new InvalidOperationException(
                    "JWT_KEY environment variable or Jwt:Key configuration is required. " +
                    "Do not use hardcoded fallback values for security.");
            }
            
            // NOTE(@Janberk): Ensure key is at least 32 characters (256 bits) for HS256
            if (jwtKeyRaw.Length < 32)
            {
                throw new InvalidOperationException(
                    $"JWT key must be at least 32 characters long. Current length: {jwtKeyRaw.Length}. " +
                    "Please set JWT_KEY environment variable or Jwt:Key configuration with a secure key.");
            }
            
            var jwtKey = jwtKeyRaw;
            var issuer = Configuration["Jwt:Issuer"] ?? "UstadAuth";
            var audience = Configuration["Jwt:Audience"] ?? "UstadClients";
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = issuer,
                        ValidAudience = audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            if (!string.IsNullOrEmpty(accessToken) && context.HttpContext.Request.Path.StartsWithSegments("/messagehub"))
                                context.Token = accessToken;
                            return Task.CompletedTask;
                        }
                    };
                });
            services.AddAuthorization();
        }

        /// <summary>
        /// Configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="env">The web host environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // NOTE(@Janberk): Enable CORS
            app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            // NOTE(@Janberk): Enable Swagger middleware
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ustad API v1");
                c.RoutePrefix = "swagger";
            });
            // NOTE(@Janberk): Use routing
            app.UseRouting();
            // NOTE(@Janberk): Use authentication
            app.UseAuthentication();
            // NOTE(@Janberk): Use authorization
            app.UseAuthorization();
            // NOTE(@Janberk): Use endpoints
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
﻿
using iPhoneBE.Data;
using iPhoneBE.Data.Data;
using iPhoneBE.Data.Mapping;
using iPhoneBE.Data.Model;
using iPhoneBE.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace iPhoneBE.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            // Swagger Configuration
            builder.Services.AddSwaggerGen(option =>
            {
                option.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
                option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
                option.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                         new string[]{}
                     }
                 });
            });

            //add jwt
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = false,
                    ValidIssuer = builder.Configuration["JWT:Issuer"],
                    ValidateAudience = false,
                    ValidAudience = builder.Configuration["JWT:Audience"],

                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecretKey"])),
                };
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Console.WriteLine("Token validated successfully");
                        var claims = context.Principal.Claims;
                        foreach (var claim in claims)
                        {
                            Console.WriteLine($"Claim: {claim.Type}: {claim.Value}");
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            //add database
            var server = builder.Configuration["server"] ?? "localhost";
            var database = builder.Configuration["database"] ?? "AppleMartDB";
            var port = builder.Configuration["port"] ?? "1433";
            var password = builder.Configuration["password"] ?? "12345";
            var user = builder.Configuration["dbuser"] ?? "sa";

            var connectionString = $"Server={server},{port};Database={database};User Id={user};Password={password};TrustServerCertificate=True;";

            Console.WriteLine("Connection String: " + builder.Configuration.GetConnectionString("DefaultConnection"));
            builder.Services.AddDbContext<AppleMartDBContext>(options =>
                options.UseSqlServer(connectionString)
                );

            builder.Services.AddIdentity<User, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
            })
                .AddEntityFrameworkStores<AppleMartDBContext>().AddDefaultTokenProviders();

            builder.Services
                .AddRepository()
                .AddServices();

            builder.Services.AddAutoMapper(typeof(AutoMapperProfile));


            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
            });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddCors();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors(opt =>
            {
                opt.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
            });

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            //check db init
            var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppleMartDBContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                context.Database.Migrate();
                //DbInitializer.Initialize(context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "A problem occurred during migration");
            }


            app.MapControllers();

            app.Run();
        }
    }
}

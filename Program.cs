using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ChatMentor.Backend.DbContext;
using ChatMentor.Backend.Data;
using NLog;
using NLog.Web;
using System.Text;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

try
{
    logger.Info("Application is starting...");

    var builder = WebApplication.CreateBuilder(args);


    // Use NLog as Logging Provider
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    // Add Services
    builder.Services.AddControllers();

    // Enable CORS
    builder.Services.AddCors(options => {
        options.AddPolicy("AllowAll",
            policy => policy.AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader());
    });

    // Add Entity Framework Core
    var serverVersion = new MySqlServerVersion(new Version(10, 4, 6));
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<ChatMentorDbContext>(options => options.UseMySql(connectionString, serverVersion));  
    
    // Add JWT Authentication
    builder.Services
        .AddAuthentication(options => { 
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; 
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; 
        })
        .AddJwtBearer(options => {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("Authentication failed: " + context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = _ =>
            {
                Console.WriteLine("Token validated");
                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];

                // If the request is for our hub...
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/chat"))
                {
                    // Read the token out of the query string
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        }; 
            options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Token"] ?? throw new InvalidOperationException())),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        }; 
            options.Authority = "Authority URL";
        });

    builder.Services.AddAuthorizationBuilder().AddPolicy("Admin", policy => policy.RequireRole("ADMIN")).AddPolicy("User", policy => policy.RequireRole("USER"));

    // Add Swagger for API documentation
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(opt => {
        opt.SwaggerDoc("v1", new OpenApiInfo { Title = "Chat_App_Api", Version = "v1" });
        opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "bearer"
        });

        opt.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type=ReferenceType.SecurityScheme,
                        Id="Bearer"
                    }
                },
                []
            }
        });
    });

    // Add SignalR
    builder.Services.AddSignalR(hubOptions =>
    {
        hubOptions.ClientTimeoutInterval = TimeSpan.FromMinutes(700);
        hubOptions.KeepAliveInterval = TimeSpan.FromSeconds(15);
    });
    
    var app = builder.Build();
    
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ChatMentorDbContext>();
        dbContext.Database.Migrate(); // Ensure database is created
        DbSeeder.Seed(dbContext); // Call seeder
    }
    
    app.UseCors("AllowAll");
    
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseAuthentication();

    app.UseRouting();

    app.UseAuthorization();

    //app.MapHub<ChatHub>("/hubs/chat");

    app.UseHttpsRedirection();

    app.MapControllers();

    app.Run();

    app.Run(); 
}
catch (Exception ex) 
{
    logger.Error(ex, "Application failed to start.");
    throw;
}
finally
{
    LogManager.Shutdown(); // Flush logs on shutdown
}

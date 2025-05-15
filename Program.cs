using System.Text;
using ChatMentor.Backend.ChatHubs;
using ChatMentor.Backend.Core.Interfaces;
using ChatMentor.Backend.Core.Interfaces.UserChatFeature;
using ChatMentor.Backend.Core.Repositories;
using ChatMentor.Backend.Core.Repositories.UserChatFeature;
using ChatMentor.Backend.Core.Services;
using ChatMentor.Backend.Core.Services.UserChatServices;
using ChatMentor.Backend.Data;
using ChatMentor.Backend.Handler;
using ChatMentor.Backend.Model;
using ChatMentor.Backend.Repositories;
using ChatMentor.Backend.Repositories.Interfaces;
using ChatMentor.Backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.ML.Tokenizers;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Web;

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
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll",
            policy => policy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
    });

    // Add Entity Framework Core
    var serverVersion = new MySqlServerVersion(new Version(10, 4, 6));
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<ChatMentorDbContext>(options => options.UseMySql(connectionString, serverVersion));
    builder.Services.Configure<OpenAiSettings>(builder.Configuration.GetSection("OpenAI"));

    // Add JWT Authentication
    builder.Services
        .AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
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
                        context.Token = accessToken;
                    return Task.CompletedTask;
                }
            };
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]
                    ?? throw new InvalidOperationException())),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

    builder.Services.AddAuthorizationBuilder().AddPolicy("Admin", policy => policy.RequireRole("ADMIN"))
        .AddPolicy("User", policy => policy.RequireRole("USER"));
    
    // Add Swagger for API documentation
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(opt =>
    {
        opt.SwaggerDoc("v1", new OpenApiInfo { Title = "ChatMentor_Api", Version = "v1" });
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
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new List<string>()
            }
        });
    });

    // Add Services
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();
    builder.Services.AddHttpContextAccessor();

    // Add Identity and other services
    builder.Services.AddScoped<IFileConverterService, FileConverterService>();
    builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
    builder.Services.AddScoped<ITagRepository, TagRepository>();
    builder.Services.AddScoped<IUserTagRepository, UserTagRepository>();
    builder.Services.AddScoped<IAiChatRepository, AiChatRepository>();
    builder.Services.AddScoped<IAiChatSessionRepository, AiChatSessionRepository>();
    builder.Services.AddScoped<IExternalAiRepository, ExternalAiRepository>();
    
    builder.Services.AddScoped<IUserConnectionsRepository, UserConnectionsRepository>();
    builder.Services.AddScoped<IMessageRepository, MessageRepository>();
    builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
    builder.Services.AddScoped<IMessageAttachmentRepository, MessageAttachmentRepository>();
    builder.Services.AddScoped<IKnowledgeBaseRepository, KnowledgeBaseRepository>();

    builder.Services.AddScoped<KnowledgeBaseService>();
    builder.Services.AddScoped<MessageService>();
    builder.Services.AddScoped<ConversationService>();
    builder.Services.AddScoped<UserConnectionService>();
    builder.Services.AddScoped<UserStatsService>();
    builder.Services.AddScoped<AiChatService>();
    builder.Services.AddScoped<UserTagService>();
    builder.Services.AddScoped<TagService>();
    builder.Services.AddScoped<DocumentService>();
    builder.Services.AddScoped<AuthService>();
    builder.Services.AddScoped<UserService>();
    builder.Services.AddScoped<TokenService>();

    
    builder.Services.Configure<FormOptions>(options =>
    {
        options.MultipartBodyLengthLimit = 30 * 1024 * 1024; // 30MB
    });
    
    // Add SignalR
    builder.Services.AddSignalR(hubOptions =>
    {
        hubOptions.ClientTimeoutInterval = TimeSpan.FromMinutes(700);
        hubOptions.KeepAliveInterval = TimeSpan.FromSeconds(15);
    });

    var app = builder.Build();

    // Add Middleware 

    // Ensure database is created and seeded
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ChatMentorDbContext>();
        dbContext.Database.Migrate(); // Ensure database is created
        DbSeeder.Seed(dbContext); // Call seeder if necessary
    }

    app.UseCors("AllowAll");

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    if (app.Environment.IsProduction())
    {
        app.UseHsts();
        app.UseExceptionHandler();
    }

    app.UseRouting();

    app.UseAuthentication();

    app.UseMiddleware<AuditLoggingMiddleware>(); // Add your middleware to the request pipeline

    app.UseAuthorization();
    
    app.MapHub<ChatHub>("/hubs/chat");
    
    app.UseHttpsRedirection();

    app.MapControllers();

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

public class OpenAiSettings
{
    public string ApiUri { get; set; }
    public string ApiKey { get; set; }
    public string ModelName { get; set; }
    public string SystemPrompt { get; set; }
    public int? MaxTokens { get; set; }
    public float? Temperature { get; set; }
    public float? TopP { get; set; }
}
namespace NicolasQuiPaieAPI.Extensions;

/// <summary>What the API is made of, one domain per member: Program.cs only puts them together.</summary>
public static class ApiSetup
{
    extension(IServiceCollection services)
    {
        /// <summary>The endpoints explorer, and Swagger in Development only, with the bearer token it sends.</summary>
        public IServiceCollection AddApiDocumentation(IHostEnvironment environment)
        {
            services.AddEndpointsApiExplorer();
            if (!environment.IsDevelopment())
            {
                return services;
            }

            return services.AddSwaggerGen(swagger =>
            {
                swagger.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Nicolas Qui Paie API",
                    Version = "v1",
                    Description = "API pour la plateforme de démocratie participative Nicolas Qui Paie - Mode Développement",
                    Contact = new OpenApiContact
                    {
                        Name = "Nicolas Qui Paie Team",
                        Email = "contact@MafyouIT.tech"
                    }
                });

                swagger.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                swagger.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    { new OpenApiSecuritySchemeReference("Bearer", document), [] }
                });
            });
        }

        /// <summary>The fronts allowed to call the API (AllowBlazorClient in every environment), and any localhost in Development.</summary>
        public IServiceCollection AddClientCors(IHostEnvironment environment)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowBlazorClient", policy =>
                {
                    policy.WithOrigins(
                            "https://localhost:7084", // NicolasQuiPaieWeb HTTPS
                            "http://localhost:5207",  // NicolasQuiPaieWeb HTTP
                            "https://localhost:5001",
                            "http://localhost:5000",
                            "https://localhost:7398", // API dans appsettings
                            "https://localhost:7051",  // API HTTPS
                            "https://happy-ocean-06624de03.2.azurestaticapps.net", // NicolasQuiPaieWeb Azure Static Web App
                            "https://*.azurestaticapps.net",
                            "https://nicolasquipaie.eu",
                            "https://nqp-efg7dvh9dzguchew.francecentral-01.azurewebsites.net" // NicolasQuiPaieWeb.Host on App Service
                          )
                          .SetIsOriginAllowedToAllowWildcardSubdomains()
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            if (environment.IsDevelopment())
            {
                services.AddCors(options =>
                {
                    options.AddPolicy("DevelopmentCors", policy =>
                    {
                        policy.SetIsOriginAllowed(origin =>
                            !string.IsNullOrWhiteSpace(origin)
                            && (origin.StartsWith("http://localhost:") || origin.StartsWith("https://localhost:")))
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                    });
                });
            }

            return services;
        }

        /// <summary>The database, its migrations history in the schema of the app.</summary>
        public IServiceCollection AddDatabase(IConfiguration configuration) =>
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                    sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", ApplicationDbContext.Schema)));

        /// <summary>
        /// Identity, then the JWT: AddIdentity makes its cookie the default scheme, and the JWT configuration that follows
        /// puts the bearer token back as the default, the only one the Blazor client sends.
        /// </summary>
        public IServiceCollection AddSecurity(IConfiguration configuration) => services
            .AddNicolasQuiPaieAuthorization()
            .AddNicolasQuiPaieJwtAuthentication(configuration);

        /// <summary>The validators, the repositories and the services of the platform.</summary>
        public IServiceCollection AddPlatform() => services
            .AddValidatorsFromAssemblyContaining<CreateProposalDtoValidator>()
            .AddScoped<IUnitOfWork, UnitOfWork>()
            .AddScoped<IProposalRepository, ProposalRepository>()
            .AddScoped<IVoteRepository, VoteRepository>()
            .AddScoped<ICommentRepository, CommentRepository>()
            .AddScoped<ICategoryRepository, CategoryRepository>()
            .AddScoped<IUserRepository, UserRepository>()
            .AddScoped<IApiLogRepository, ApiLogRepository>()
            .AddScoped<IProposalService, ProposalService>()
            .AddScoped<IVotingService, VotingService>()
            .AddScoped<IJwtService, JwtService>()
            .AddScoped<IEmailService, EmailService>()
            .AddScoped<IAnalyticsService, AnalyticsService>();
    }

    extension(WebApplication app)
    {
        /// <summary>Swagger (Development, under /swagger), CORS, HTTPS unless disabled, authentication, authorization, the request log.</summary>
        public WebApplication UseApiPipeline()
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(swagger =>
                {
                    swagger.SwaggerEndpoint("/swagger/v1/swagger.json", "Nicolas Qui Paie API v1 - Development");
                    swagger.DocumentTitle = "Nicolas Qui Paie API - Development Mode";
                });
                app.UseCors("DevelopmentCors");
            }
            else
            {
                app.UseCors("AllowBlazorClient");
            }

            if (!app.Configuration.GetValue<bool>("DisableHttpsRedirection"))
            {
                app.UseHttpsRedirection();
            }

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCustomSerilogRequestLogging();
            return app;
        }

        /// <summary>One line in the log once the database is ready.</summary>
        public WebApplication AnnounceStart()
        {
            Log.Information("🚀 Nicolas Qui Paie API started in {Environment} mode. Swagger: {SwaggerEnabled}. SQL Logging: Enabled with custom ApiLog table. 🔐 Role-based Authorization: Active",
                app.Environment.EnvironmentName, app.Environment.IsDevelopment() ? "Available" : "Disabled");
            return app;
        }
    }
}

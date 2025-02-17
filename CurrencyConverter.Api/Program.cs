using Asp.Versioning;
using CurrencyConverter.Api.Common;
using CurrencyConverter.Api.Middlewares;
using CurrencyConverter.Api.ModelBinders;
using CurrencyConverter.Data;
using CurrencyConverter.Data.Models;
using CurrencyConverter.Services;
using CurrencyConverter.Services.AppServices.Configuration.Dtos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;

namespace CurrencyConverter.Api
{
    public partial class Program
    {
        public async static Task Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            // Apply Aspire Service Defaults
            builder.AddServiceDefaults();

            // TODO: README file - For production, use Azure Key Vault to store secrets.

            #region Configuration
            // TODO: Mention ASPNETCORE_ENVIRONMENT in the README file.
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            // Load configuration from multiple sources depending on the environment (Dev, Test, Production).
            builder.Configuration
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: false, reloadOnChange: true)
                .AddUserSecrets<Program>()
                .AddEnvironmentVariables();

            // Get CurrencyConverterConfiguration section from configuration and validate it.
            CurrencyConverterConfigurationDto configuration = builder.Configuration.GetSection("CurrencyConverterConfiguration").Get<CurrencyConverterConfigurationDto>()
                ?? throw new InvalidConfigurationException("CurrencyConverter root Configuration node is missing");
            configuration.Validate();

            #endregion

            // Add custom services to the dependency injection container.
            CurrencyConverterServicesDiMapper.MapAppServices(builder.Services);

            // Register the database context.
            RegisterDbContext(builder);

            // Configure the identity system for user and role management.
            AddIdentitySystem(builder);

            // Configure JWT authentication and authorization policies.
            ConfigureJwtAuthenticationAndAuthorizationPolicies(builder, configuration.Jwt);

            // Configure API versioning.
            ConfigureApiVersioning(builder);

            // Configure caching for improved performance.
            ConfigureCaching(builder);

            // Configure Swagger for API documentation.
            ConfigureSwagger(builder);

            // Configure exception handling middleware.
            ConfigureExceptionsHandling(builder);

            // Add controllers with custom model binders.
            builder.Services.AddControllers(options =>
            {
                options.ModelBinderProviders.Insert(0, new CustomDateTimeModelBinderProvider());
            });

            // Add API explorer and Swagger support.
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            WebApplication app = builder.Build();

            // Use custom exception handling middleware.
            app.UseExceptionHandler();

            // Map default endpoints for the application.
            app.MapDefaultEndpoints();

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI();

            if (app.Environment.IsDevelopment())
            {
                // Seed initial identity data (users and roles) for development.
                await SeedIdentityInitialData(app);
            }
            else
            {
                #region Enforce HTTPS

                // Use HSTS to enforce HTTPS in production.
                app.UseHsts();

                // Redirect HTTP requests to HTTPS.
                app.UseHttpsRedirection();

                #endregion
            }

            // Enable authentication and authorization.
            app.UseAuthentication();
            app.UseAuthorization();

            // Apply rate limiting globally.
            app.UseRateLimiter();

            // Map controllers to endpoints.
            app.MapControllers();

            #region Custom Middlewares

            // Use custom middleware Circuit Breaker.
            app.UseMiddleware<CircuitBreakerMiddleware>();

            // Use custom middleware for HTTP request logging.
            app.UseMiddleware<HttpRequestLoggingMiddleware>();

            #endregion

            // Run the application.
            await app.RunAsync();
        }

        #region Privates

        /// <summary>
        /// Registers the database context for the application.
        /// </summary>
        /// <param name="builder">The web application builder.</param>
        private static void RegisterDbContext(WebApplicationBuilder builder)
        {
            // Use an in-memory database for development.
            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddDbContext<CurrencyConverterDbContext>(options =>
                    options.UseInMemoryDatabase("CurrencyConverterDb"));
            }
            else
            {
                // Use a SQL Server database for production.
                // builder.Services.AddDbContext<CurrencyConverterDbContext>(options => options.UseSqlServer(configuration.ConnectionStrings.DbConnection));

                // TODO: Apply database migrations in production.
                /*
                 * Run the following commands in the Package Manager Console to create and apply migrations:
                 * 1- Add a migration: "Add-Migration InitialIdentitySchema"
                 * 2- Update the database: "Update-Database"
                 */
            }
        }

        /// <summary>
        /// Configures the identity system for user and role management.
        /// </summary>
        /// <param name="builder">The web application builder.</param>
        private static void AddIdentitySystem(WebApplicationBuilder builder)
        {
            builder.Services.AddIdentity<CurrencyConverterUser, IdentityRole>()
                .AddEntityFrameworkStores<CurrencyConverterDbContext>()
                .AddDefaultTokenProviders();
        }

        /// <summary>
        /// Configures JWT authentication and authorization policies.
        /// </summary>
        /// <param name="builder">The web application builder.</param>
        /// <param name="configuration">The JWT configuration settings.</param>
        private static void ConfigureJwtAuthenticationAndAuthorizationPolicies(WebApplicationBuilder builder, JwtConfigurationDto configuration)
        {
            // Convert the secret key to a byte array.
            byte[] key = Encoding.ASCII.GetBytes(configuration.SecretKey);

            // Configure JWT authentication.
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration.Issuer,
                    ValidAudience = configuration.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });

            // Add authorization policies for roles.
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy(CurrencyConverterAuthorizationPolicy.USER, policy => policy.RequireRole(CurrencyConverterAuthorizationRole.USER));
                options.AddPolicy(CurrencyConverterAuthorizationPolicy.ADMIN, policy => policy.RequireRole(CurrencyConverterAuthorizationRole.ADMIN));
            });
        }

        /// <summary>
        /// Configures API versioning for the application.
        /// </summary>
        /// <param name="builder">The web application builder.</param>
        private static void ConfigureApiVersioning(WebApplicationBuilder builder)
        {
            builder.Services.AddApiVersioning(option =>
            {
                option.AssumeDefaultVersionWhenUnspecified = true; // Use the default version if none is specified.
                option.DefaultApiVersion = new ApiVersion(1, 0); // Set the default API version.
                option.ReportApiVersions = true; // Report API versions in the response headers.

                // Combine URL-based and header-based versioning.
                option.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader(),
                    new HeaderApiVersionReader(CurrencyConverterCustomHeader.API_VERSION));
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV"; // Format for version numbers.
                options.SubstituteApiVersionInUrl = true; // Resolve routing conflicts.
            });
        }

        /// <summary>
        /// Configures caching for improved performance.
        /// </summary>
        /// <param name="builder">The web application builder.</param>
        private static void ConfigureCaching(WebApplicationBuilder builder)
        {
            // Add Redis distributed caching.
            builder.AddRedisDistributedCache("distributedCache");

            // Add hybrid caching (in-memory + distributed).
#pragma warning disable EXTEXP0018 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            builder.Services.AddHybridCache();
#pragma warning restore EXTEXP0018 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        }

        /// <summary>
        /// Configures exception handling middleware and problem details service.
        /// </summary>
        /// <param name="builder">The web application builder.</param>
        private static void ConfigureExceptionsHandling(WebApplicationBuilder builder)
        {
            // Register the custom problem details writer.
            builder.Services.AddSingleton<IProblemDetailsWriter, CustomProblemDetailsWriter>();

            // Configure problem details for error responses.
            builder.Services.AddProblemDetails();

            // Add custom exception handling middleware.
            builder.Services.AddExceptionHandler<ExceptionHandlingMiddleware>();

            // Suppress automatic model validation errors.
            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });
        }

        /// <summary>
        /// Configures Swagger for API documentation with JWT authentication support.
        /// </summary>
        /// <param name="builder">The web application builder.</param>
        private static void ConfigureSwagger(WebApplicationBuilder builder)
        {
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Currency Converter API", Version = "v1" });

                // Configure JWT authentication in Swagger.
                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "JWT Authentication",
                    Description = "Enter JWT Bearer token",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };

                // Include XML comments in Swagger documentation.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                // Configure Swagger options.
                c.DescribeAllParametersInCamelCase();
                c.OrderActionsBy(x => x.RelativePath);
                c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        securityScheme, Array.Empty<string>()
                    }
                });
            });
        }

        /// <summary>
        /// Seeds initial identity data (users and roles) into the database.
        /// </summary>
        /// <param name="app">The web application.</param>
        private static async Task SeedIdentityInitialData(WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var services = scope.ServiceProvider;
            var userManager = services.GetRequiredService<UserManager<CurrencyConverterUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            // Seed roles.
            if (!await roleManager.RoleExistsAsync(CurrencyConverterAuthorizationRole.ADMIN))
            {
                await roleManager.CreateAsync(new IdentityRole(CurrencyConverterAuthorizationRole.ADMIN));
            }
            if (!await roleManager.RoleExistsAsync(CurrencyConverterAuthorizationRole.USER))
            {
                await roleManager.CreateAsync(new IdentityRole(CurrencyConverterAuthorizationRole.USER));
            }

            // Seed an admin user.
            var adminUser = new CurrencyConverterUser { UserName = "admin1@currencyconverter.com", Email = "admin1@currencyconverter.com" };
            if (await userManager.FindByEmailAsync(adminUser.Email) == null)
            {
                await userManager.CreateAsync(adminUser, "Admin@123");
                await userManager.AddToRoleAsync(adminUser, CurrencyConverterAuthorizationRole.ADMIN);
            }

            // Seed a regular user.
            var regularUser = new CurrencyConverterUser { UserName = "user1@currencyconverter.com", Email = "user1@currencyconverter.com" };
            if (await userManager.FindByEmailAsync(regularUser.Email) == null)
            {
                await userManager.CreateAsync(regularUser, "User@123");
                await userManager.AddToRoleAsync(regularUser, CurrencyConverterAuthorizationRole.USER);
            }
        }

        #endregion
    }
}
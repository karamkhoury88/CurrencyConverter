
using Asp.Versioning;
using CurrencyConverter.Api.Common;
using CurrencyConverter.Api.Middlewares;
using CurrencyConverter.Data;
using CurrencyConverter.Data.Models;
using CurrencyConverter.Services;
using CurrencyConverter.Services.Configuration;
using CurrencyConverter.Services.Configuration.Dtos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

namespace CurrencyConverter.Api
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            //TODO: README file For production, we need to use Azure Key Vault to store the secrets.

            #region Configuration

            // Load configuration from multiple sources
            builder.Configuration
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
#if DEBUG
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
#endif
                .AddUserSecrets<Program>()
                .AddEnvironmentVariables();

            #endregion

            // Get CurrencyConverterConfiguration section from configuration and validate it
            CurrencyConverterConfigurationDto configuration = builder.Configuration.GetSection("CurrencyConverterConfiguration").Get<CurrencyConverterConfigurationDto>()
                ?? throw new InvalidOperationException("Configuration are missing");
            configuration.Validate();

            // Add our custom services to services collection (for DI)
            CurrencyConverterServicesDiMapper.MapAppServices(builder.Services);

            // Register Db Context
            RegisterDbContext(builder);

            // Adds the default identity system configuration for the specified User and Role types.  
            AddIdentitySystem(builder);

            //Configure JWT authentication and add authorization policies
            ConfigureJwtAuthenticationAndAuthorizationPolicies(builder, configuration.Jwt);

            // Configure API versioning 
            ConfigureApiVersioning(builder);

            // Configure Caching layer
            ConfigureCaching(builder);

            // Configure Rate Limiting
            ConfigureRateLimiting(builder, anonymousUserRateLimitingConfiguration: configuration.AnonymousUserRateLimitingConfiguration, authenticatedUsersConfiguration: configuration.AuthenticatedUserRateLimitingConfiguration);

            // Add Swagger with JWT auth support
            ConfigureSwagger(builder);

            // Add OpenTelemetry for Application Performance Monitoring
            AddOpenTelemetry(builder);

            // Register the IHttpClientFactory
            RegisterHttpClientFactory(builder);

            builder.Services.AddControllers();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            WebApplication app = builder.Build();

            // Configure the HTTP request pipeline.
            // Use swagger documentations
            app.UseSwagger();
            app.UseSwaggerUI();

            if (app.Environment.IsDevelopment())
            {
                // Seed Identity Initial Data
                // TODO: README file - Mention this point 
                await SeedIdentityInitialData(app);
            }
            else
            {
                #region Enforce HTTPS

                // HSTS Middleware (UseHsts) to send HTTP Strict Transport Security Protocol (HSTS) headers to clients.
                // The default HSTS value is 30 days. we may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();

                //  Adds middleware for redirecting HTTP Requests to HTTPS.
                app.UseHttpsRedirection();

                #endregion
            }

            // Adds Authentication Middleware to enable authentication capabilities.
            app.UseAuthentication();

            // Adds Authorization Middleware to enable Authorization capabilities.
            app.UseAuthorization();

            // Apply rate limiter globally
            app.UseRateLimiter();

            app.MapControllers();

            #region Custom Middlewares

            // Http Request Logging
            app.UseMiddleware<HttpRequestLoggingMiddleware>();

            #endregion
            
            await app.RunAsync();
        }

        #region Privates 

        /// <summary>
        ///  Register CurrencyConverter DbContext
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration"></param>
        private static void RegisterDbContext(WebApplicationBuilder builder)
        {
            // TODO: README file - Mention this point 
            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddDbContext<CurrencyConverterDbContext>(options =>
                options.UseInMemoryDatabase("CurrencyConverterDb"));
            }
            else
            {
                // builder.Services.AddDbContext<CurrencyConverterDbContext>(options => options.UseSqlServer(configuration.ConnectionStrings.DbConnection));

                //TODO: Apply Database Migrations
                /*
                 * Run the following commands in the Package Manager Console to create and apply migrations:
                 * 1- Add a migration: "Add-Migration InitialIdentitySchema"
                 * 2- Update the database: "Update-Database"
                 */
            }
        }

        /// <summary>
        /// Adds the default identity system configuration for the specified User and Role types.
        /// </summary>
        /// <param name="builder"></param>
        private static void AddIdentitySystem(WebApplicationBuilder builder)
        {
            builder.Services.AddIdentity<CurrencyConverterUser, IdentityRole>()
                .AddEntityFrameworkStores<CurrencyConverterDbContext>()
                .AddDefaultTokenProviders();
        }

        /// <summary>
        ///  Configure Jwt Authentication And Authorization Policies
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration"></param>
        private static void ConfigureJwtAuthenticationAndAuthorizationPolicies(WebApplicationBuilder builder, JwtConfigurationDto configuration)
        {

            // convert the secretKey string value to bytes array
            byte[] key = Encoding.ASCII.GetBytes(configuration.SecretKey);

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
            //  Add Authorization Policies
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy(CurrencyConverterAuthorizationPolicy.USER, policy => policy.RequireRole(CurrencyConverterAuthorizationRole.USER));
                options.AddPolicy(CurrencyConverterAuthorizationPolicy.ADMIN, policy => policy.RequireRole(CurrencyConverterAuthorizationRole.ADMIN));
            });
        }

        /// <summary>
        /// Configure Api Versioning to support both URL-Based Versioning and Header-Based Versioning
        /// </summary>
        /// <param name="builder"></param>
        private static void ConfigureApiVersioning(WebApplicationBuilder builder)
        {
            builder.Services.AddApiVersioning(option =>
            {
                option.AssumeDefaultVersionWhenUnspecified = true; //This ensures if client doesn't specify an API version. The default version should be considered. 
                option.DefaultApiVersion = new ApiVersion(1, 0); //This we set the default API version
                option.ReportApiVersions = true; //The allow the API Version information to be reported in the client  in the response header. This will be useful for the client to understand the version of the API they are interacting with.

                //This says how the API version should be read from the client's request, 
                //a custom header named "api-version", to be set with version number in client before request the endpoints.
                //or in the url
                option.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader(),
                    new HeaderApiVersionReader(CurrencyConverterCustomHeader.API_VERSION));
            })
                .AddApiExplorer(options =>
                {
                    options.GroupNameFormat = "'v'VVV"; //The format of version number “‘v’major[.minor][-status]”
                    options.SubstituteApiVersionInUrl = true; //This will help us to resolve the ambiguity when there is a routing conflict due to routing template one or more end points are same.
                });
        }

        /// <summary>
        /// Add Caching layer to our system, to boost the performance
        /// </summary>
        /// <param name="builder"></param>
        private static void ConfigureCaching(WebApplicationBuilder builder)
        {
            //TODO: README - Add support for Redis cash for distributed cache support or use the new hybrid cache to have both in memory and distributed caching layers in one interface 
            builder.Services.AddMemoryCache();
        }

        /// <summary>
        /// Configure Rate Limiting For authenticated users from any IP, and Rate Limiting For any call from a specific IP
        /// </summary>
        /// <param name="builder"></param>
        private static void ConfigureRateLimiting(WebApplicationBuilder builder, RateLimitingConfigurationDto authenticatedUsersConfiguration, RateLimitingConfigurationDto anonymousUserRateLimitingConfiguration)
        {
            // The RateLimiter middleware is designed to work efficiently in distributed scenarios,
            // and we can replace the in-memory partitioner with a distributed cache (e.g., Redis) if needed.
            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                // Global limiter that applies different policies based on authentication status
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                {
                    if (httpContext.User?.Identity?.IsAuthenticated == true)
                    {
                        // Use user ID for authenticated users
                        var userKey = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown-user";

                        return RateLimitPartition.GetFixedWindowLimiter(
                            partitionKey: userKey,
                            factory: _ => new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = authenticatedUsersConfiguration.PermitLimit,
                                Window = TimeSpan.FromMinutes(authenticatedUsersConfiguration.Window)
                            });
                    }
                    else
                    {
                        // Use IP address for non-authenticated users
                        var ipKey = httpContext.Request.Headers["X-Forwarded-For"].ToString()
                                    ?? httpContext.Connection.RemoteIpAddress?.ToString()
                                    ?? "unknown-ip";

                        return RateLimitPartition.GetFixedWindowLimiter(
                            partitionKey: ipKey,
                            factory: _ => new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = anonymousUserRateLimitingConfiguration.PermitLimit,
                                Window = TimeSpan.FromMinutes(anonymousUserRateLimitingConfiguration.Window)
                            });
                    }
                });

            });
        }

        /// <summary>
        /// Add OpenTelemetry for Application metrics, tracing and log Monitoring
        /// </summary>
        /// <param name="builder"></param>
        private static void AddOpenTelemetry(WebApplicationBuilder builder)
        {
            builder.Services.AddOpenTelemetry()

                // Configure the resource (metadata) for the telemetry data
                .ConfigureResource(resource =>
                    // Add a service name to identify the source of the telemetry data
                    resource.AddService("CurrencyConverterApi"))

                // Configure metrics collection
                .WithMetrics(metrics =>
                {
                    // Add instrumentation for ASP.NET Core to collect metrics about incoming HTTP requests
                    metrics.AddAspNetCoreInstrumentation();

                    // Add instrumentation for HttpClient to collect metrics about outgoing HTTP requests
                    metrics.AddHttpClientInstrumentation();

                    // Export metrics to the console (for debugging and development purposes)
                    metrics.AddConsoleExporter();
                    // We can set this to work with aspire dashboard
                    // metrics AddOtlpExporter(options => options.Endpoint = new Uri("http://CurrencyConverterApi.dashboard:18889"));
                })

                // Configure tracing collection
                .WithTracing(tracing =>
                {
                    // Add instrumentation for ASP.NET Core to collect traces about incoming HTTP requests
                    tracing.AddAspNetCoreInstrumentation();

                    // Add instrumentation for HttpClient to collect traces about outgoing HTTP requests
                    tracing.AddHttpClientInstrumentation();

                    // Export traces to the console (for debugging and development purposes)
                    tracing.AddConsoleExporter();

                    // Optionally, export traces to an OTLP (OpenTelemetry Protocol) endpoint
                    // This can be used to send traces to an OpenTelemetry Collector or a backend like Jaeger
                    // Uncomment and configure the endpoint to work with the Aspire dashboard or other OTLP-compatible systems
                    // tracing.AddOtlpExporter(options => options.Endpoint = new Uri("http://CurrencyConverterApi.dashboard:18889"));
                });

            // Configure logging to use OpenTelemetry
            builder.Logging.AddOpenTelemetry(logging =>
            {
                // Export logs to the console (for debugging and development purposes)
                logging.AddConsoleExporter();

                // Optionally, export logs to an OTLP (OpenTelemetry Protocol) endpoint
                // This can be used to send logs to an OpenTelemetry Collector or a backend like Elasticsearch or Seq
                // Uncomment and configure the endpoint to work with the Aspire dashboard or other OTLP-compatible systems
                // logging.AddOtlpExporter(options => options.Endpoint = new Uri("http://CurrencyConverterApi.dashboard:18889"));


                // Include the formatted message in the log records
                logging.IncludeFormattedMessage = true;

                // Include log scopes (additional context) in the log records
                logging.IncludeScopes = true;

                // Parse and include structured log state values in the log records
                logging.ParseStateValues = true;
            });

        }


        /// <summary>
        /// Register the IHttpClientFactory, To Manages the life-cycle of HttpClient instances, preventing resource exhaustion issues associated with frequent instantiation.
        /// </summary>
        /// <param name="builder"></param>
        private static void RegisterHttpClientFactory(WebApplicationBuilder builder)
        {
            builder.Services.AddHttpClient();
        }

        /// <summary>
        /// Configure Swagger documentation that support JWT authentication
        /// </summary>
        /// <param name="builder"></param>
        private static void ConfigureSwagger(WebApplicationBuilder builder)
        {
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Currency Converter API", Version = "v1" });

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
        /// Seed some initial users and roles into the in-memory database
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        private static async Task SeedIdentityInitialData(WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var services = scope.ServiceProvider;
            var userManager = services.GetRequiredService<UserManager<CurrencyConverterUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            // Seed roles
            if (!await roleManager.RoleExistsAsync(CurrencyConverterAuthorizationRole.ADMIN))
            {
                await roleManager.CreateAsync(new IdentityRole(CurrencyConverterAuthorizationRole.ADMIN));
            }
            if (!await roleManager.RoleExistsAsync(CurrencyConverterAuthorizationRole.USER))
            {
                await roleManager.CreateAsync(new IdentityRole(CurrencyConverterAuthorizationRole.USER));
            }

            // Seed an admin user
            var adminUser = new CurrencyConverterUser { UserName = "admin1@currencyconverter.com", Email = "admin1@currencyconverter.com" };
            if (await userManager.FindByEmailAsync(adminUser.Email) == null)
            {
                await userManager.CreateAsync(adminUser, "Admin@123");
                await userManager.AddToRoleAsync(adminUser, CurrencyConverterAuthorizationRole.ADMIN);
            }

            // Seed a regular user
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

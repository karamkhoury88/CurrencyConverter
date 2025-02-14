
using Asp.Versioning;
using CurrencyConverter.Api.Common;
using CurrencyConverter.Api.Middlewares;
using CurrencyConverter.Data;
using CurrencyConverter.Data.Models;
using CurrencyConverter.Services;
using CurrencyConverter.Services.Configuration.Dtos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Polly;
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
            builder.AddServiceDefaults();

            //TODO: README file For production, we need to use Azure Key Vault to store the secrets.

            #region Configuration

            // Load configuration from multiple sources
            builder.Configuration
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddUserSecrets<Program>()
                .AddEnvironmentVariables();

            if (builder.Environment.IsDevelopment())
            {
                builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
            }

            // Get CurrencyConverterConfiguration section from configuration and validate it
            CurrencyConverterConfigurationDto configuration = builder.Configuration.GetSection("CurrencyConverterConfiguration").Get<CurrencyConverterConfigurationDto>()
            ?? throw new InvalidConfigurationException("CurrencyConverter root Configuration node is missing");
            configuration.Validate();

            #endregion

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

            // Register the IHttpClientFactory
            RegisterHttpClientFactory(builder);

            // Configure exception handling logic
            ConfigureExceptionsHandling(builder);

            _ = builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            builder.Services.AddControllers();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            WebApplication app = builder.Build();

            app.MapDefaultEndpoints();

            // Configure the HTTP request pipeline.
            // Use swagger documentations
            app.UseSwagger();
            app.UseSwaggerUI();

            // Let the app use our exception handling middleware
            app.UseExceptionHandler();

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
            builder.AddRedisDistributedCache("distributedCache");
#pragma warning disable EXTEXP0018 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            builder.Services.AddHybridCache();
#pragma warning restore EXTEXP0018 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        }

        private static void ConfigureExceptionsHandling(WebApplicationBuilder builder)
        {
            // Configure ProblemDetails approach 
            builder.Services.AddProblemDetails(option =>
            {
                option.CustomizeProblemDetails = context => context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method}{context.HttpContext.Request.Path}";
            });

            // Add our exception handling middleware
            builder.Services.AddExceptionHandler<ExceptionHandlingMiddleware>();

            // Suppress Automatic model validation
            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });
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

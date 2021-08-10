using com.ataxlab.azure.table.retention.models.automapper;
using com.ataxlab.azure.table.retention.models.control;
using com.ataxlab.azure.table.retention.models.models.auth;
using com.ataxlab.azure.table.retention.services.azuremanagement;
using com.ataxlab.azure.table.retention.services.dashboardapi;
using com.ataxlab.functions.table.retention.dashboard.Models;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.Azure.Management.Network.Fluent.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace com.ataxlab.functions.table.retention.dashboard
{
    /// <summary>
    /// handle camelCase PropertiesOf[].properties for typescript
    /// </summary>
    public class CamelCasingPropertiesFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema model, SchemaFilterContext context)
        {
            model.Properties =
                model.Properties.ToDictionary(d => d.Key.Substring(0, 1).ToLower() + d.Key.Substring(1), d => d.Value);
            if (context.Type.IsEnum)
            {
                model.Enum.Clear();
                
                Enum.GetNames(context.Type)
                    .ToList()
                    .ForEach(n => model.Enum.Add(new OpenApiString(n)));
            }

        }
    }

    /// <summary>
    /// as per https://stackoverflow.com/questions/29701573/how-to-omit-methods-from-swagger-documentation-on-webapi-using-swashbuckle
    /// </summary>
    class RemoveVerbsFilter : IDocumentFilter
    {
   
        /// <summary>
        /// filter openaapi spec
        /// </summary>
        /// <param name="swaggerDoc"></param>
        /// <param name="context"></param>
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var pathsToRemove = swaggerDoc.Paths
                            .Where(pathItem => pathItem.Key.Contains("/MicrosoftIdentity"))
                            .ToList();

            foreach (var item in pathsToRemove)
            {
                swaggerDoc.Paths.Remove(item.Key);
            }
        }
    }
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            try
            {
                // as per https://devblogs.microsoft.com/aspnet/forwarded-headers-middleware-updates-in-net-core-3-0-preview-6/
                if (string.Equals(
                    Environment.GetEnvironmentVariable("ASPNETCORE_FORWARDEDHEADERS_ENABLED"),
                    "true", StringComparison.OrdinalIgnoreCase))
                {
                    services.Configure<ForwardedHeadersOptions>(options =>
                    {
                        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                               ForwardedHeaders.XForwardedHost | // as per https://soapfault.com/2020/02/24/asp-net-core-reverse-proxy-and-x-forwarded-headers/
                            ForwardedHeaders.XForwardedProto;
                        // Only loopback proxies are allowed by default.
                        // Clear that restriction because forwarders are enabled by explicit 
                        // configuration.
                        options.ForwardLimit = 2; // as per https://soapfault.com/2020/02/24/asp-net-core-reverse-proxy-and-x-forwarded-headers/
                        options.KnownNetworks.Clear();
                        options.KnownProxies.Clear();
                    });
                }

                EnsureServicesConfiguration(services);


                // holy crap trailing slashes matter 
                var applianceBaseUrl = Configuration.GetValue<String>("ApplianceBaseUrl");
                applianceBaseUrl = applianceBaseUrl == null ? String.Empty :
                                applianceBaseUrl.EndsWith("/api/") ? applianceBaseUrl.Substring(0, applianceBaseUrl.Length - 5) : applianceBaseUrl;
                applianceBaseUrl = applianceBaseUrl == null ? String.Empty :
                                applianceBaseUrl.EndsWith("/api") ? applianceBaseUrl.Substring(0, applianceBaseUrl.Length - 4) : applianceBaseUrl;

                services.AddCors(options =>
                {
                    options.AddPolicy(name: "CorsPolicy",
                                      builder =>
                                      {
                                          builder.WithOrigins
                                          ("https://localhost:44349", "https://localhost", "http://localhost",
                                          "https://localhost:5001",
                                                              "https://logon.microsoft.com", "http://192.168.10.138:4200",
                                                              "https://login.microsoftonline.com",
                                                              "https://localhost:7071",
                                                              applianceBaseUrl)
                                          .AllowAnyHeader()
                                          .AllowAnyMethod()
                                          .SetIsOriginAllowed(origin => true)
                                          //.AllowAnyOrigin(); is incompatible with .allowCredentials() 
                                          .AllowCredentials();
                                      });
                });

                // disable performance counters
                // as per https://stackoverflow.com/questions/42890719/how-to-disable-standard-performance-counters-in-application-insights
                var serviceDescriptor = services.FirstOrDefault(descriptor => descriptor.ImplementationType == typeof(PerformanceCollectorModule));
                services.Remove(serviceDescriptor);
            }
            catch (Exception e)
            {
                Console.WriteLine("failure starting up with exception {0}", e.Message);
            }

            // services.AddApplicationInsightsTelemetry(Configuration["APPINSIGHTS_CONNECTIONSTRING"]);
            // as per https://docs.microsoft.com/en-us/azure/azure-monitor/app/sampling
            // disable adaptive sampling
            services.AddApplicationInsightsTelemetry(opts =>
            {
                opts.EnableAdaptiveSampling = false;
                opts.InstrumentationKey = Configuration["APPINSIGHTS_CONNECTIONSTRING"];

            });
        }

        private void EnsureServicesConfiguration(IServiceCollection services)
        {
            var clientSecret = Configuration.GetValue<String>("AzureAD:ClientSecret");
            var clientId = Configuration.GetValue<String>("AzureAD:ClientId");
            // holy crap trailing slashes matter 
            var baseUrl = Configuration.GetValue<String>("ApplianceBaseUrl");
            baseUrl = baseUrl == null ? String.Empty :
                        baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/";


            // one per appdomain
            services.AddAutoMapper(typeof(AutoMapperProfile));
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IAuthenticationProvider, OnBehalfOfMsGraphAuthenticationProvider>();

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Storage Account Cleaner For Azure", Version = "v1" });
                // as per https://stackoverflow.com/questions/29701573/how-to-omit-methods-from-swagger-documentation-on-webapi-using-swashbuckle
                c.DocumentFilter<RemoveVerbsFilter>();
                // as per https://stackoverflow.com/questions/60954335/camelcase-in-swagger-documentation
                c.SchemaFilter<CamelCasingPropertiesFilter>();
                c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
                c.IgnoreObsoleteProperties();
                
            });

            // as per https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/master/3-WebApp-multi-APIs
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.Secure = CookieSecurePolicy.SameAsRequest;
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
                options.HandleSameSiteCookieCompatibility();

            });

            // setup for requesting scopes
            var applianceAppUri = Configuration.GetValue<String>("ApplianceAppUri");
            var scopes = new TableRetentionApplianceScopes() { ApplianceAppUri = applianceAppUri };
            services.AddSingleton<TableRetentionApplianceScopes>(scopes);

            // as per https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/master/3-WebApp-multi-APIs
            services.AddAuthentication((opts) =>
            {
                opts.DefaultScheme = OpenIdConnectDefaults.AuthenticationScheme;
                opts.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                opts.DefaultSignInScheme = OpenIdConnectDefaults.AuthenticationScheme;
                

            })

            .AddJwtBearer(o =>
            {
                o.Events.OnMessageReceived = OnMessageReceived;
                o.Authority = Configuration.GetValue<string>("AzureAD:Authority"); // "https://login.microsoftonline.com/{tenantguid}/v2.0";
                //Require tokens be saved in the AuthenticationProperties on the request
                //We need the token later to get another token
                o.SaveToken = true;
                o.Events.OnChallenge = OnChallenge;
                o.Audience = Configuration.GetValue<string>("AzureAD:Audience");
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    //Both the client id and app id URI of this API should be valid audiences
                    // ValidAudiences = new List<string> { Configuration.GetValue < string>("AzureAD:Audience")}
                };
            }).AddMicrosoftIdentityWebApp(Configuration.GetSection("AzureAd"), subscribeToOpenIdConnectMiddlewareDiagnosticsEvents: true)
            .EnableTokenAcquisitionToCallDownstreamApi() //(scopes.AllScopes)
            // .EnableTokenAcquisitionToCallDownstreamApi(new List<string>() { scopes.DefaultApplianceScope[0]}) //(scopes.AllScopes)
            //.EnableTokenAcquisitionToCallDownstreamApi(new List<string>() { "https://management.azure.com/user_impersonation" }) //(scopes.AllScopes)

            .AddDownstreamWebApi("azuretableretentionappliance", (o) =>
             {
                 o.BaseUrl = baseUrl;
                 o.Scopes = scopes.DefaultApplianceScope[0];


             })
            .AddInMemoryTokenCaches();

            services.AddHttpContextAccessor();

            services.AddHttpClient<IAzureManagementAPIClient, AzureManagementAPIClient>()

                .ConfigureHttpClient((c) =>
                {
                    c.BaseAddress = new Uri("https://management.azure.com");
                })
                .SetHandlerLifetime(TimeSpan.FromMinutes(5))  //Set lifetime to five minutes
                .AddPolicyHandler(GetRetryPolicy());

            services.AddHttpClient<ITableRetentionDashboardAPI, TableRetentionDashboardAPI>()
                    .ConfigureHttpClient((x) =>
                    {

                        x.BaseAddress = new Uri(baseUrl);
                    })
                    .SetHandlerLifetime(TimeSpan.FromMinutes(5))  //Set lifetime to five minutes
                    .AddPolicyHandler(GetRetryPolicy());


            services.AddDistributedMemoryCache();
            services.AddSession(opts =>
            {
                
                opts.IdleTimeout = TimeSpan.FromMinutes(24);
                opts.Cookie.HttpOnly = false;
                opts.Cookie.IsEssential = true;

            });

            services.AddControllersWithViews(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
              

            }).AddRazorRuntimeCompilation()
                .AddJsonOptions(opts =>
                {
                    // as per https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/1269
                    // https://gist.github.com/regisdiogo/27f62ef83a804668eb0d9d0f63989e3e
                    // https://medium.com/@jrhodes.home/exposing-enums-through-swagger-in-net-core-api-616d3727a02c
                    opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
 
                    opts.JsonSerializerOptions.IgnoreNullValues = true;
                    
                })
                .AddMicrosoftIdentityUI();
            // as per https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/master/3-WebApp-multi-APIs


            services.AddRazorPages()
                .AddRazorRuntimeCompilation()
                 .AddMicrosoftIdentityUI();

            services.AddDbContext<ScaffoldingContext>(options =>
        options.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=azuretableretentionapplaince"));
        }

        private Task OnChallenge(JwtBearerChallengeContext arg)
        {
            int i = 0;
            return Task.CompletedTask;
        }

        private Task OnMessageReceived(Microsoft.AspNetCore.Authentication.JwtBearer.MessageReceivedContext arg)
        {
            var x = arg.Response;
            return Task.CompletedTask;
        }

        private Task OnTokenValidated(Microsoft.AspNetCore.Authentication.JwtBearer.TokenValidatedContext arg)
        {
            var securityToken = arg.SecurityToken;
            return Task.CompletedTask;
        }

        static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,
                                                                            retryAttempt)));
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            var configuration = app.ApplicationServices.GetService<TelemetryConfiguration>();

            var builder = configuration.DefaultTelemetrySink.TelemetryProcessorChainBuilder;


            // Using fixed rate sampling as per https://docs.microsoft.com/en-us/azure/azure-monitor/app/sampling
            double fixedSamplingPercentage = 10;
            builder.UseSampling(fixedSamplingPercentage);

            builder.Build();

            if (env.IsDevelopment())
            {
                app.UseExceptionHandler("/");
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                // app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            #region angular integration
            //// corresponds with /angular* paths
            //// expected by angular components
            //// that are otherwise self-contained
            //// and configured at runtime by calling
            //// configuration webapi
            //var cwd = Directory.GetParent(Directory.GetCurrentDirectory());
            //var angularPath = Path.Combine(cwd.FullName, "angular-components\\dashboard\\dist\\dashboard");
            //var angularAssetsPath = Path.Combine(cwd.FullName, "angular-components\\dashboard\\dist\\dashboard\\assets");

            //app.UseStaticFiles(new StaticFileOptions()
            //{

            //    FileProvider = new PhysicalFileProvider(angularPath),
            //    RequestPath = new PathString("/angular")
            //});

            //app.UseStaticFiles(new StaticFileOptions()
            //{

            //    FileProvider = new PhysicalFileProvider(angularAssetsPath),
            //    RequestPath = new PathString("/angular-assets")
            //});

            #endregion angular integration

            // as per https://seankilleen.com/2020/06/solved-net-core-azure-ad-in-docker-container-incorrectly-uses-an-non-https-redirect-uri/
            // attempted fix for aad redict url issues to docker apps running on http: you cannot redirect to a routeable http from azuread
            app.UseForwardedHeaders();
            app.UseRouting();
            app.UseCors("CorsPolicy");

            app.UseCookiePolicy();

            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Storage Account Cleaner For Azure");
               ;
            });


            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}

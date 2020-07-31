using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using SimpleSpa.NgClient.HttpHandlers;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace SimpleSpa.NgClient
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });

            services.AddAuthorization(options => 
            {
                options.AddPolicy(
                    "JoeAdmin", 
                    policyBuilder => 
                    {
                        policyBuilder.RequireAuthenticatedUser();
                        policyBuilder.RequireClaim("given_name", "Joe");
                        policyBuilder.RequireClaim("family_name", "Ipe");
                        policyBuilder.RequireRole("Admin");
                    });
            });

            services.AddHttpContextAccessor();
            services.AddTransient<BearerTokenHandler>();

            services.AddHttpClient("SampleA_APIClient", client =>
            {
                client.BaseAddress = new System.Uri("https://localhost:44300/");
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
            }).AddHttpMessageHandler<BearerTokenHandler>();

            services.AddHttpClient("SampleB_APIClient", client =>
            {
                client.BaseAddress = new System.Uri("https://localhost:44301/");
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
            }).AddHttpMessageHandler<BearerTokenHandler>(); ;

            services.AddHttpClient("IDPClient", client => 
            {
                client.BaseAddress = new System.Uri("https://localhost:5001/");
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
            });

            services.AddSession();

            services.AddAuthentication(options => 
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options => 
                {
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.Authority = "https://localhost:5001/";
                    options.ClientId = "simplespaclient";
                    options.ClientSecret = "secret";
                    options.ResponseType = "code";
                    options.UsePkce = true;
                    //options.CallbackPath = new PathString("...")
                    //options.SignedOutCallbackPath = new PathString("...")
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("address");
                    options.Scope.Add("roles");
                    options.Scope.Add("simpleaapi");
                    options.Scope.Add("simplebapi");
                    options.Scope.Add("offline_access"); //refresh token related
                    options.SaveTokens = true;
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.ClaimActions.MapJsonKey("role", "role");
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = JwtClaimTypes.GivenName,
                        RoleClaimType = JwtClaimTypes.Role,
                    };

                    options.Events = new Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectEvents
                    {
                        OnRedirectToIdentityProviderForSignOut = (context) =>
                        {
                            var protocolMessage = context.ProtocolMessage;
                            var param = "";
                            foreach (var parameter in protocolMessage.Parameters)
                            {
                                param += $"{parameter.Key}={parameter.Value}$";
                            }
                            var url = $"{protocolMessage.IssuerAddress}?{param}x-client-SKU={protocolMessage.SkuTelemetryValue}&x-client-ver=5.5.0.0";

                            context.HttpContext.Session.SetString("LogoutUrl", url);

                            return Task.FromResult(0);
                        }
                    };
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            app.UseRouting();

            app.UseSession();

            app.UseAuthentication();
            app.UseAuthorization();

            app.Use(async (context, next) =>
            {
                if (!context.User.Identity.IsAuthenticated)
                {
                    await context.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme);
                }
                else
                {
                    await next();
                }
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }
    }
}

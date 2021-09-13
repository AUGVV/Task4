using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Task4.Data;

namespace Task4
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        string TakeSecretKey(string name, string code)
        {
            var KeyVaultUrl = $"https://task4vault.vault.azure.net/secrets/" + name + "/" + code;
            AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
            KeyVaultClient keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            var secret = keyVaultClient.GetSecretAsync(KeyVaultUrl).Result.Value;
            Debug.WriteLine(secret);
            return secret;
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
         

            AppDomain.CurrentDomain.SetData("DataDirectory", Environment.CurrentDirectory);
            Debug.WriteLine(Environment.CurrentDirectory);
            Debug.WriteLine(AppDomain.CurrentDomain.GetData("DataDirectory"));

            services.AddDbContext<UsersContext>(options => options.UseSqlServer(Configuration.GetConnectionString("UsersDataConnection")));
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddAuthentication().AddFacebook(facebookOptions =>
             {
                facebookOptions.AppId = TakeSecretKey("AuthenticationFacebookAppId", "f8b5f49f4aa44dadba87c2cf6d486935");
                 facebookOptions.AppSecret = TakeSecretKey("AuthenticationFacebookAppSecret", "6c4a0e89c3334b19bd0c742f15ac8db9");
             });
            services.AddAuthentication().AddMicrosoftAccount(microsoftOptions =>
            {
                microsoftOptions.ClientId = TakeSecretKey("AuthenticationMicrosoftClientId", "18d66fcf43f34290951189688f6f781b");             
                microsoftOptions.ClientSecret = TakeSecretKey("AuthenticationMicrosoftClientSecret", "644b755a791344d3bc97279dd7fe66ec");
            });
            services.AddAuthentication().AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = TakeSecretKey("AuthenticationGoogleClientId", "3fec56e8be604e509557ea2510a4414c");
                googleOptions.ClientSecret = TakeSecretKey("AuthenticationGoogleClientSecret", "0ff1ba0a09644eefac0a458de5f3dbff");
            });
        }
 

            // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
            public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
           {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

             app.UseAuthentication();
             app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}

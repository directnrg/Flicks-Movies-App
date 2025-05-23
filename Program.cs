using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Flicks_App.Models;
using Flicks_App.Areas.Identity.Data;
using dotenv.net;
using Amazon.Runtime;
using Amazon.SimpleSystemsManagement.Model;
using Amazon.SimpleSystemsManagement;
using Flicks_App.AWS;
using System.Diagnostics;

namespace Flicks_App
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DotEnv.Load();

            var builder = WebApplication.CreateBuilder(args);
            //var connectionString = builder.Configuration.GetConnectionString("Connection2RDS") ?? throw new InvalidOperationException("Connection string 'ConnectionTo2RDS' not found.");
            var connectionString = builder.Configuration.GetConnectionString("DBLocalLab") ?? throw new InvalidOperationException("Connection string 'DBLocalLab' not found.");

            builder.Services.AddDbContext<FlicksDBContext>(options =>
                  options.UseSqlServer(connectionString));
                //options.UseSqlServer(ParameterStore.GetConnectionStringFromParameterStore().Result.Value));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<CustomUser>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddEntityFrameworkStores<FlicksDBContext>();
            builder.Services.AddControllersWithViews();

            builder.Services.Configure<IdentityOptions>(options =>

            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;

                // Email Confirmation Settings
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = false;

                // User settings.
                options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;
            });

            builder.Services.ConfigureApplicationCookie(options =>

            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(10);

                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.SlidingExpiration = true;

            });

            //adding session to store sensitive or ignored data from database.
            builder.Services.AddSession();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
                //for more info when an error happens.
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseSession();
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}");

            app.MapRazorPages();
            app.Run();
        }
    }
}
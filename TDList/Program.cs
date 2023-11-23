using Microsoft.EntityFrameworkCore;
using TDList.Models;
using Microsoft.AspNetCore.Mvc.Razor;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using Microsoft.AspNetCore.Identity;
using TDList.Servises;
using TDList.Services;
using System.Configuration;

namespace TDList
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddTransient<IEmailSendler,EmailSendler>();
            builder.Services.AddControllersWithViews(options =>
            {
                options.CacheProfiles.Add("Cashing", new CacheProfile() { Location = ResponseCacheLocation.Any, Duration = 300 });
                options.CacheProfiles.Add("NoCashing", new CacheProfile() { Location = ResponseCacheLocation.None, NoStore = true });
            });
            builder.Services.AddResponseCompression(options =>
            {
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "image/svg+xml" });
            });
            builder.Services.AddControllersWithViews()
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix);
            builder.Services.AddLocalization(options =>
            {
                options.ResourcesPath = "Resources";
            });
            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                {
                    new CultureInfo("ru-RU"),
                    new CultureInfo("en-US"),
                    new CultureInfo("zh-CN"),
                };
                options.DefaultRequestCulture = new RequestCulture("ru-RU");
                options.SupportedUICultures = supportedCultures;
            });
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            string connection = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<TDLContext>(options => options.UseNpgsql(connection))
               .AddIdentity<User, IdentityRole<int>>(options =>
               {
                   options.Password.RequiredLength = 5;
                   options.Password.RequireNonAlphanumeric = false;
                   options.Password.RequireLowercase = false;
                   options.Password.RequireUppercase = false;
                   options.Password.RequireDigit = false;
               }).AddEntityFrameworkStores<TDLContext>();
            builder.Services.AddTransient<UserService>();
            builder.Services.AddDbContext<TDLContext>(options => options.UseNpgsql(connection));
            var app = builder.Build();
            app.UseResponseCompression();
            var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
            using (var scope = scopeFactory.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
                var roles = new[] {"Admin", "Manager", "Member" };
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                        await roleManager.CreateAsync(new IdentityRole<int>(role));
                }
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                await AdminInitial.SeedAdminUser(roleManager, userManager);
            }
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.Use(async (context, next) =>
            {
                var selectedCulture = context.Request.Cookies[CookieRequestCultureProvider.DefaultCookieName];
                if (!string.IsNullOrEmpty(selectedCulture))
                {
                    Console.WriteLine($"Selected Culture: {selectedCulture}");
                    var requestCulture = CookieRequestCultureProvider.ParseCookieValue(selectedCulture);
                    var cultureInfo = new CultureInfo(requestCulture!.Cultures.FirstOrDefault().ToString() ?? "en-US" ?? "zh-CN");
                    Thread.CurrentThread.CurrentCulture = cultureInfo;
                    Thread.CurrentThread.CurrentUICulture = cultureInfo;
                }
                await next();
            });

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseRequestLocalization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Goals}/{action=Index}/{id?}");

            app.Run();
        }
    }
}

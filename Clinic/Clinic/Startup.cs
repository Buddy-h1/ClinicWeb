using Clinic.Areas.Identity.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Clinic.Logging;
using Clinic.Database;

namespace Clinic
{
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
            var CustomIdentityDbContextConnectionString = Configuration.GetConnectionString("CustomIdentityDbContextConnection") ?? throw new InvalidOperationException("Connection string 'CustomIdentityDbContextConnection' not found.");
            var MainClinicDatabaseConnectionString = Configuration.GetConnectionString("MainClinicDatabaseConnection") ?? throw new InvalidOperationException("Connection string 'MainClinicDatabaseConnection' not found.");
            DatabaseManager.ConnectionString = MainClinicDatabaseConnectionString;

            services.AddRazorPages();

            services.AddDbContext<CustomIdentityDbContext>(options => options.UseSqlServer(CustomIdentityDbContextConnectionString));
            services.AddDbContext<ClinicTestContext>(options => options.UseSqlServer(MainClinicDatabaseConnectionString));

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = false;
            });

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);

                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.SlidingExpiration = true;
            });

            services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
                            .AddRoleManager<RoleManager<IdentityRole>>()
                            .AddEntityFrameworkStores<CustomIdentityDbContext>()
                            .AddDefaultUI()
                            .AddDefaultTokenProviders()
                            .AddErrorDescriber<RussianIdentityErrorDescriber>();

            // Add services to the container.
            services.AddControllersWithViews();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options => //CookieAuthenticationOptions
                {
                    options.LoginPath = new Microsoft.AspNetCore.Http.PathString("/Identity/Account/Login");
                });

            // If using Kestrel:
            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            // If using IIS:
            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            services.AddMvc().AddJsonOptions(o =>
            {
                o.JsonSerializerOptions.PropertyNamingPolicy = null;
                o.JsonSerializerOptions.DictionaryKeyPolicy = null;
                o.JsonSerializerOptions.WriteIndented = true;
            });

            string logFileName = Configuration["LogFileName"] ?? "";
            Logger.Init(logFileName);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //using (var scope = app.ApplicationServices.CreateScope())
            //{
            //    var dbContext1 = scope.ServiceProvider.GetRequiredService<CustomIdentityDbContext>();
            //    dbContext1.Database.Migrate();
            //}

            // Configure the HTTP request pipeline.
            if (!env.IsDevelopment())
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
using Book_Store.Data;
using Book_Store.Models;
using Book_Store.Repository;
using Book_Store.Repository.Interface;
using Book_Store.View_Models.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace Book_Store
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();


            //Register AppDbContext
            builder.Services.AddDbContext<AppDbContext>(op => op.UseSqlServer(
                builder.Configuration.GetConnectionString("conn")
                ));


            builder.Services.ConfigureApplicationCookie(op =>
            {
                op.ExpireTimeSpan = TimeSpan.FromDays(6);
                op.LogoutPath = "/Home/Landing";
                op.AccessDeniedPath = "/Home/Landing";
            });

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>( op =>
            {
                op.Password.RequireLowercase = false;
                op.Password.RequireNonAlphanumeric = false;
                op.Password.RequireUppercase = false;
                op.Password.RequireDigit = false;
                op.Password.RequiredLength = 8;
            })
               .AddEntityFrameworkStores<AppDbContext>();

            builder.Services.AddMemoryCache();


            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("...");



            builder.Services.AddSession();

            //Register Services 
            builder.Services.AddScoped<IAccountRepo, AccountRepo>(); 
            builder.Services.AddScoped<IHomeRepo, HomeRepo>(); 
            builder.Services.AddScoped<IUserDashboardRepo, UserDashboardRepo>();
            builder.Services.AddScoped<IDashboardRepo, DashboardRepo>();
            builder.Services.AddScoped<IEmailRepo, EmailRepo>();

            builder.Services.Configure<EmailConfigureModel>(builder.Configuration.GetSection("EmailConfigure"));

            var app = builder.Build();

            
            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseStatusCodePagesWithRedirects("/Home/ErrorPage/{0}");
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }


            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseRouting();

            app.UseSession();


            //Register Stripe Configration
            StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();


            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Landing}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}

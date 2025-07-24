using Aplicacion.API.Consumer;
using Aplicacion.Modelos.Favorite;
using Aplicacion.Modelos.Identity;
using Aplicacion.Modelos.Implementations;
using Aplicacion.Modelos.Suscription;
using Aplicacion.MVC.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

namespace Aplicacion.MVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Crud<Aplicacion.Modelos.Musica>.EndPoint = "https://localhost:7095/api/Musics";
            Crud<Aplicacion.Modelos.Album>.EndPoint = "https://localhost:7095/api/Albums";
            Crud<Aplicacion.Modelos.Playlist>.EndPoint = "https://localhost:7095/api/Playlists";

            Crud<Download>.EndPoint = "https://localhost:7095/api/Downloads";
            Crud<Follow>.EndPoint = "https://localhost:7095/api/Follows";
            Crud<Notificacion>.EndPoint = "https://localhost:7095/api/Notifications";
            Crud<SubscriptionPlan>.EndPoint = "https://localhost:7095/api/SubscriptionPlans";
            Crud<UserSubscription>.EndPoint = "https://localhost:7095/api/UserSubscriptions";

            Crud<FavoritoMusica>.EndPoint = "https://localhost:7095/api/FavoriteMusics";
            Crud<FavoritoArtista>.EndPoint = "https://localhost:7095/api/FavoriteArtists";

            Crud<User>.EndPoint = "https://localhost:7095/api/Users";


            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("SqlConnection")
                ?? throw new InvalidOperationException("Connection string not found.");

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));

            //  Identity directo en MVC
            builder.Services.AddDefaultIdentity<User>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
            })
            .AddDefaultUI()
            .AddRoles<Role>()
            .AddEntityFrameworkStores<AppDbContext>();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
            });




            builder.Services.AddScoped<Aplicacion.MVC.Services.IEmailService, Aplicacion.MVC.Services.EmailService>();
            builder.Services.AddHostedService<SubscriptionExpiryService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();



            // Add services to the container.
            builder.Services.AddControllersWithViews();


            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 100_000_000; // 100MB
                options.ValueLengthLimit = int.MaxValue;
                options.ValueCountLimit = int.MaxValue;
            });

            builder.Services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = 100_000_000; // 100MB
            });



            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }



            app.UseHttpsRedirection();
            app.UseStaticFiles();

            //
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "Storage")),
                RequestPath = "/files"
            });



            app.UseRouting();

            //
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");



            app.Run();
        }
    }
}

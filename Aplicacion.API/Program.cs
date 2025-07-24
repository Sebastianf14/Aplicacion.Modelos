using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Aplicacion.Modelos.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity;

namespace Aplicacion.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("SqlConnection")
                ?? throw new InvalidOperationException("Connection string 'AppDbContext' not found.")));

            //Identity
            builder.Services.AddDefaultIdentity<User>()
                .AddRoles<Role>()
                .AddEntityFrameworkStores<AppDbContext>();

            //// Add services to the container.
            builder.Services
            .AddControllers()
            .AddNewtonsoftJson(
                options => options.SerializerSettings.ReferenceLoopHandling
                = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            );
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(c => {
                c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
                // Otra lógica personalizada si es necesario
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();


            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}

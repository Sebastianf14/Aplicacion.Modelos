using Aplicacion.API.Consumer;
using Aplicacion.Modelos.Suscription;

namespace Aplicacion.MVC.Services
{
    public class SubscriptionExpiryService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public SubscriptionExpiryService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                        // Verificar suscripciones que expiran en 7 días
                        var expiringDate = DateTime.Now.AddDays(7);
                        var allSubscriptions = Crud<UserSubscription>.GetAll();

                        var expiringSoon = allSubscriptions.Where(s =>
                            s.IsActive &&
                            s.SubscriptionPlan?.Price > 0 &&
                            s.EndDate.Date == expiringDate.Date
                        ).ToList();

                        foreach (var subscription in expiringSoon)
                        {
                            await notificationService.NotifySubscriptionExpiringAsync(
                                subscription.UserId,
                                subscription.SubscriptionPlan?.Name ?? "Premium",
                                subscription.EndDate
                            );
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error en SubscriptionExpiryService: {ex.Message}");
                }

                // Ejecutar una vez al día
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }

}

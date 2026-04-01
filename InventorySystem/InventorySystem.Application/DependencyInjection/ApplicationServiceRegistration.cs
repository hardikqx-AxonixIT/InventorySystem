using InventorySystem.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;

namespace InventorySystem.Application.DependencyInjection
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddValidatorsFromAssembly(typeof(ApplicationServiceRegistration).Assembly);

            services.AddScoped<IStockService, StockService>();
            services.AddScoped<IAdjustmentService, AdjustmentService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IAdvancedErpService, AdvancedErpService>();
            return services;
        }
    }
}

using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace StockFlow.Api.Extensions
{
    public static class HealthCheckExtensions
    {
        /// <summary>
        /// /health/live  : ứng dụng còn chạy không (KHÔNG kiểm DB) → App Service dùng để quyết định restart
        /// /health/ready : sẵn sàng phục vụ chưa (CÓ kiểm DB)     → CD dùng để smoke test sau deploy
        /// </summary>
        public static WebApplication MapStockFlowHealthChecks(this WebApplication app) {
            app.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = _ => false
            });
            app.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready")
            });
            return app;        
        }
    }
}

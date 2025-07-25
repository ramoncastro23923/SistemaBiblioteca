using Microsoft.Extensions.Diagnostics.HealthChecks;
using SistemaBiblioteca.Data;
using System.Threading;
using System.Threading.Tasks;

namespace SistemaBiblioteca.HealthChecks
{
    public class BibliotecaContextHealthCheck : IHealthCheck
    {
        private readonly BibliotecaContext _dbContext;

        public BibliotecaContextHealthCheck(BibliotecaContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Verifica se pode conectar ao banco de dados
                var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);

                if (canConnect)
                {
                    return HealthCheckResult.Healthy(
                        "Conexão com o banco de dados estabelecida com sucesso");
                }

                return HealthCheckResult.Unhealthy(
                    "Não foi possível estabelecer conexão com o banco de dados");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(
                    "Falha ao verificar a conexão com o banco de dados",
                    exception: ex);
            }
        }
    }
}
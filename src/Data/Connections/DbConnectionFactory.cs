using CrossCutting.Concerns;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System.Data;

namespace Data.Connections
{
    /// <summary>
    /// Fabrica de conexões controlada pelo lifecycle da DI
    /// </summary>
    /// <remarks>
    /// Controller
    /// </remarks>
    /// <param name="configuration"></param>
    public class DbConnectionFactory(IConfiguration configuration) : IDbConnectionFactory
    {

        private readonly IConfiguration _configuration = configuration;

        /// <summary>
        /// Cria a conexão default
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public IDbConnection CreateConnection()
        {
            return new MySqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        }
    }
}

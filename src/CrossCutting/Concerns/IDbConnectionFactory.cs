using System.Data;

namespace CrossCutting.Concerns
{
    /// <summary>
    /// Fabrica de conexões controlada pelo lifecycle da DI
    /// </summary>
    public interface IDbConnectionFactory
    {
        /// <summary>
        /// Cria a conexão default
        /// </summary>
        /// <returns></returns>
        IDbConnection CreateConnection();
    }
}

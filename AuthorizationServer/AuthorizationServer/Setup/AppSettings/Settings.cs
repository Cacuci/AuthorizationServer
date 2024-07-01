namespace AuthorizationServer.Setup.AppSettings
{
    public class Settings
    {
        /// <summary>
        /// Chave para criação/criptografia do token
        /// </summary>
        public string SecretKey { get; set; }

        /// <summary>
        /// Collection de aplicações
        /// </summary>                
        public IEnumerable<Application> Applications { get; set; }

        /// <summary>
        /// Collection de escopos
        /// </summary>                
        public IEnumerable<Scope> Scopes { get; set; }

        /// <summary>
        /// Collection de conexões
        /// </summary>                
        public IEnumerable<Connection> ConnectionStrings { get; set; }

        /// <summary>
        /// Retorna um System.TimeSpan que representa em minutos o tempo valido para o Token        
        /// </summary>
        /// <param name="clientId">
        /// ID de identificação do cliente
        /// </param>
        /// <returns>
        /// Um objeto que representa valor
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// ID do cliente informado não encontrado
        /// </exception>
        public TimeSpan TokenLifetime(string clientId)
        {
            var result = Applications.FirstOrDefault(item => item.ID.Equals(clientId, StringComparison.CurrentCultureIgnoreCase));

            ArgumentNullException.ThrowIfNull(result, clientId);

            return TimeSpan.FromMinutes(result.Expires);
        }

        /// <summary>
        /// Retorna um IEnumerable de string que representa as audiencias permitidas       
        /// </summary>
        /// <param name="clientId">
        /// ID de identificação do cliente
        /// </param>
        /// <returns>
        /// Uma lista de string que contém as audiencias
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// ID do cliente informado não encontrado
        /// </exception>
        public IEnumerable<string> Audiences(string clientId)
        {
            var result = Applications.FirstOrDefault(item => item.ID.Equals(clientId, StringComparison.CurrentCultureIgnoreCase));

            ArgumentNullException.ThrowIfNull(result, clientId);

            return result.Audiences.Select(item => item);
        }
    }
}
namespace AuthorizationServer.Setup.AppSettings
{
    public class Application
    {
        public string ID { get; set; }

        public EGrantType GrantType { get; set; }

        public string ClientSecret { get; set; }

        public int Expires { get; set; }

        public IEnumerable<string> Audiences { get; set; }

        public IEnumerable<string> Scopes { get; set; }
    }
}
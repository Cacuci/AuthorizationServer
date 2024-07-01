namespace AuthorizationServer.Setup.AppSettings
{
    public record Connection
    {
        public string Name { get; set; }
        public string ConnectionString { get; set; }
    }
}
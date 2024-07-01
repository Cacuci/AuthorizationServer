namespace AuthorizationServer.Setup.AppSettings
{
    public record Scope
    {
        public string Name { get; set; }

        /// <summary>
        /// Collection de escopos
        /// </summary>                
        public IEnumerable<string> Resources { get; set; }
    }
}
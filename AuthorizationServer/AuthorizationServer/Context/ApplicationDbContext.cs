using AuthorizationServer.Setup.AppSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AuthorizationServer.Context
{
    public class ApplicationDbContext(DbContextOptions options, IOptionsSnapshot<Settings> settings) : DbContext(options)
    {
        private readonly Settings _settings = settings.Value;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (!options.IsConfigured)
            {
                var connection = _settings.ConnectionStrings.SingleOrDefault(item => item.Name.Equals(EConnection.OpenIddict.ToString(), StringComparison.CurrentCultureIgnoreCase));

                ArgumentNullException.ThrowIfNull(connection, nameof(OpenIddict));

                options.UseSqlServer(connection.ConnectionString);
            }
        }
    }
}
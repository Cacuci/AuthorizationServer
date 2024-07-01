using AuthorizationServer.Context;
using Microsoft.IdentityModel.Tokens;

namespace AuthorizationServer.Setup.Extensions
{
    public static class OpenIddictExtensions
    {
        public static void RegisterOpenIddict(this IServiceCollection services, IConfiguration Configuration)
        {
            services.AddOpenIddict()

               // Register the OpenIddict core components.
               .AddCore(options =>
               {
                   // Configure OpenIddict to use the Entity Framework Core stores and models.
                   // Note: call ReplaceDefaultEntities() to replace the default OpenIddict entities.
                   options.UseEntityFrameworkCore()
                          .UseDbContext<ApplicationDbContext>();

                   // Enable Quartz.NET integration.
                   options.UseQuartz();
               })

               // Register the OpenIddict server components.
               .AddServer(options =>
               {
                   // Enable the token endpoint.
                   options.SetTokenEndpointUris("connect/token")
                          .SetIntrospectionEndpointUris("connect/introspect")
                          .SetVerificationEndpointUris("connect/verify");

                   // Enable the password flow.
                   options.AllowPasswordFlow();

                   // Accept anonymous clients (i.e clients that don't send a client_id).
                   options.AcceptAnonymousClients();

                   // Register the encryption credentials. This sample uses a symmetric
                   // encryption key that is shared between the server and the Api2 sample
                   // (that performs local token validation instead of using introspection).
                   //
                   // Note: in a real world application, this encryption key should be
                   // stored in a safe place (e.g in Azure KeyVault, stored as a secret).
                   options.AddEncryptionKey(new SymmetricSecurityKey(Convert.FromBase64String(Configuration.GetSection("Settings:SecretKey").Value)));

                   // Register the signing and encryption credentials.
                   options.AddDevelopmentEncryptionCertificate()
                          .AddDevelopmentSigningCertificate()
                          .DisableAccessTokenEncryption();

                   // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
                   options.UseAspNetCore()
                          .EnableTokenEndpointPassthrough();
               })

               // Register the OpenIddict validation components.
               .AddValidation(options =>
               {
                   // Import the configuration from the local OpenIddict server instance.
                   options.UseLocalServer();

                   // Register the ASP.NET Core host.
                   options.UseAspNetCore();
               });
        }
    }
}
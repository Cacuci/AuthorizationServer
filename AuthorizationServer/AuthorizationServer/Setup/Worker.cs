using AuthorizationServer.Context;
using AuthorizationServer.Setup.AppSettings;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace AuthorizationServer.Setup
{
    public class Worker(IServiceProvider serviceProvider) : IHostedService
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await using var scope = _serviceProvider.CreateAsyncScope();

            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var options = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<AppSettings.Settings>>();

            await context.Database.EnsureCreatedAsync(cancellationToken);

            await RegisterApplicationsAsync(scope.ServiceProvider, options);

            await CreateScopesAsync(scope.ServiceProvider, options);

            static async Task RegisterApplicationsAsync(IServiceProvider provider, IOptionsSnapshot<AppSettings.Settings> options)
            {
                var manager = provider.GetRequiredService<IOpenIddictApplicationManager>();

                foreach (var client in options.Value.Applications)
                {
                    if (client.GrantType == EGrantType.Password)
                    {
                        if (await manager.FindByClientIdAsync(client.ID) is null)
                        {
                            var descriptor = new OpenIddictApplicationDescriptor
                            {
                                ClientId = client.ID,
                                Permissions =
                                {
                                    Permissions.Endpoints.Token,
                                    Permissions.GrantTypes.Password,
                                    Permissions.Scopes.Email,
                                    Permissions.Scopes.Roles
                                }
                            };

                            foreach (var scope in client.Scopes)
                            {
                                descriptor.Permissions.Add(Permissions.Prefixes.Scope + scope);
                            }

                            await manager.CreateAsync(descriptor);
                        }
                    }

                    if (client.GrantType == EGrantType.ClientCredentials)
                    {
                        // API
                        if (await manager.FindByClientIdAsync(client.ID) == null)
                        {
                            var descriptor = new OpenIddictApplicationDescriptor
                            {
                                ClientId = client.ID,
                                ClientSecret = client.ClientSecret,
                                Permissions =
                                {
                                    Permissions.Endpoints.Introspection
                                }
                            };

                            _ = await manager.CreateAsync(descriptor);
                        }
                    }
                }
            }

            static async Task CreateScopesAsync(IServiceProvider provider, IOptionsSnapshot<AppSettings.Settings> options)
            {

                var manager = provider.GetRequiredService<IOpenIddictScopeManager>();

                foreach (var scope in options.Value.Scopes)
                {
                    if (await manager.FindByNameAsync(scope.Name) is null)
                    {
                        var descriptor = new OpenIddictScopeDescriptor
                        {
                            Name = scope.Name,
                        };

                        foreach (var item in scope.Resources)
                        {
                            descriptor.Resources.Add(item);
                        }

                        _ = await manager.CreateAsync(descriptor);
                    }
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
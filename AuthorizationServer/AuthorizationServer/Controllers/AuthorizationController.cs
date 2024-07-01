using AuthorizationServer.Models;
using AuthorizationServer.Setup.AppSettings;
using Dapper;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace AuthorizationServer.Controllers
{
    public class AuthorizationController(IOptionsSnapshot<Setup.AppSettings.Settings> options) : Controller
    {
        private readonly Setup.AppSettings.Settings _options = options.Value;

        [AllowAnonymous]
        [HttpPost("~/connect/token"), IgnoreAntiforgeryToken, Produces("application/json")]
        public async Task<IActionResult> ExchangeAsync()
        {
            var properties = new AuthenticationProperties();

            var request = HttpContext.GetOpenIddictServerRequest() ??
                          throw new InvalidOperationException("A solicitação do OpenID Connect não pode ser recuperada.");

            ClaimsPrincipal claimsPrincipal;

            if (request.IsPasswordGrantType())
            {
                var connection = _options.ConnectionStrings?.SingleOrDefault(item => item.Name.Equals(EConnection.WM.ToString(), StringComparison.CurrentCultureIgnoreCase));

                await using (var resource = new SqlConnection(connection?.ConnectionString))
                {
                    var user = await resource.QuerySingleOrDefaultAsync<User>(@"select ID, Login, Name, Password, ID_UserClass as UserClass 
                                                                                  from UserAcess 
                                                                                 where Login = @login", new { login = request.Username });
                    if (user == null)
                    {
                        properties = new AuthenticationProperties(new Dictionary<string, string>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                                "Usuário e/ou senha inválido(s)."
                        });

                        return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                    }

                    if (!user.IsValidPassword(request.Password))
                    {
                        properties = new AuthenticationProperties(new Dictionary<string, string>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "Usuário e/ou senha inválido(s)."
                        });

                        return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                    }

                    // Create the claims-based identity that will be used by OpenIddict to generate tokens.
                    var identity = new ClaimsIdentity(
                        authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                        nameType: Claims.Name,
                        roleType: Claims.Role);

                    // Add the claims that will be persisted in the tokens.
                    identity.SetClaim(Claims.Subject, user.ID.ToString())
                            .SetClaim(Claims.Name, user.Name)
                            .SetClaim(Claims.PreferredUsername, user.Name)
                            .SetClaim(Claims.Role, user.UserClass.ToString());

                    // Set the list of scopes granted to the client application.
                    identity.SetScopes(new[]
                    {
                        //"lmtapi",
                        Scopes.OpenId,
                        Scopes.Email,
                        Scopes.Profile,
                        Scopes.Roles,
                    }.Intersect(request.GetScopes()));

                    identity.SetAudiences(_options.Audiences(request.ClientId));

                    identity.SetDestinations(GetDestinations);

                    claimsPrincipal = new ClaimsPrincipal(identity);

                    claimsPrincipal.SetAccessTokenLifetime(_options.TokenLifetime(request.ClientId));

                    //Returning a SignInResult will ask OpenIddict to issue the appropriate access / identity tokens.
                    return SignIn(claimsPrincipal, properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }
            }
            else
            {
                throw new InvalidOperationException("O tipo de concessão especificado não é compatível.");
            }
        }

        private static IEnumerable<string> GetDestinations(Claim claim)
        {
            // Note: by default, claims are NOT automatically included in the access and identity tokens.
            // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
            // whether they should be included in access tokens, in identity tokens or in both.

            switch (claim.Type)
            {
                case Claims.Name or Claims.PreferredUsername:
                    yield return Destinations.AccessToken;

                    if (claim.Subject.HasScope(Scopes.Profile))
                        yield return Destinations.IdentityToken;

                    yield break;

                case Claims.Email:
                    yield return Destinations.AccessToken;

                    if (claim.Subject.HasScope(Scopes.Email))
                        yield return Destinations.IdentityToken;

                    yield break;

                case Claims.Role:
                    yield return Destinations.AccessToken;

                    if (claim.Subject.HasScope(Scopes.Roles))
                        yield return Destinations.IdentityToken;

                    yield break;

                // Never include the security stamp in the access and identity tokens, as it's a secret value.
                case "AspNet.Identity.SecurityStamp": yield break;

                default:
                    yield return Destinations.AccessToken;
                    yield break;
            }
        }
    }
}
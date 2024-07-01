using AuthorizationServer.DTOs;
using AuthorizationServer.Models;
using AuthorizationServer.Setup;
using AuthorizationServer.Setup.AppSettings;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Net;

namespace AuthorizationServer.Controllers
{
    public class AccountController(IOptionsSnapshot<Settings> options) : Controller
    {
        private readonly Settings _options = options.Value;

        // POST: /Account/Login
        [HttpPost("~/account/login"), IgnoreAntiforgeryToken, Produces("application/json")]
        [AllowAnonymous]
        public async Task<ActionResult<Response>> Login([FromBody] AuthRequest request)
        {
            var connection = _options.ConnectionStrings?.SingleOrDefault(item => item.Name.Equals(EConnection.WM.ToString(), StringComparison.CurrentCultureIgnoreCase));

            await using (var resource = new SqlConnection(connection?.ConnectionString))
            {
                if (request.Login.Equals("", StringComparison.CurrentCultureIgnoreCase) &&
                    request.Password.Equals("", StringComparison.CurrentCultureIgnoreCase))
                {
                    return Ok(new Response(status: (int)HttpStatusCode.OK));
                }

                var user = await resource.QuerySingleOrDefaultAsync<User>(@"select Login, Password
                                                                              from UserAcess 
                                                                             where Login = @login", new { login = request.Login });

                if (user is null)
                {
                    throw new DomainException("Usuário e/ou senha inválido(s)");
                }

                if (!user.IsValidPassword(request.Password))
                {
                    throw new DomainException("Usuário e/ou senha inválido(s)");
                }

                return Ok(new Response(status: (int)HttpStatusCode.OK));
            }
        }
    }
}
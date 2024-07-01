using AuthorizationServer.Context;
using AuthorizationServer.Setup;
using AuthorizationServer.Setup.AppSettings;
using AuthorizationServer.Setup.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Quartz;

namespace AuthorizationServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
            => Configuration = configuration;

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen(setup =>
            {
                setup.SwaggerDoc("v1", new OpenApiInfo { Title = "Authorization", Version = "v1", Description = "Aplicação de identidade/autorização baseado no padrão OpenID Connect. A documentação encontra-se disponível em /.well-known/openid-configuration, onde você pode encontrar metadados e links para todos os endpoints." });

                setup.CustomSchemaIds(s => s.ToString());

                var path = AppContext.BaseDirectory;

                foreach (var name in Directory.GetFiles(path, "*.xml"))
                {
                    setup.IncludeXmlComments(filePath: name);
                }
            });

            services.Configure<Settings>(Configuration.GetSection(nameof(Settings)));

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                // Register the entity sets needed by OpenIddict.
                // Note: use the generic overload if you need
                // to replace the default OpenIddict entities.
                options.UseOpenIddict();
            });

            // OpenIddict offers native integration with Quartz.NET to perform scheduled tasks
            // (like pruning orphaned authorizations/tokens from the database) at regular intervals.
            services.AddQuartz(options =>
            {
                options.UseMicrosoftDependencyInjectionJobFactory();
                options.UseSimpleTypeLoader();
                options.UseInMemoryStore();
            });

            // Register the Quartz.NET service and configure it to block shutdown until jobs are complete.
            services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

            services.RegisterOpenIddict(Configuration);

            services.AddAuthorization();

            // Register the worker responsible for creating and seeding the SQL database.
            // Note: in a real world application, this step should be part of a setup script.
            services.AddHostedService<Worker>();
        }

        public void Configure(WebApplication app, IWebHostEnvironment env)
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseDeveloperExceptionPage();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
        }
    }
}
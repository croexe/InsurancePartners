using Partners.Api.Endpoints;
using Partners.Api.Hubs;
using Serilog;

namespace Partners.Api.Extensions;

internal static class WebApplicationExtensions
{
    public static WebApplication ApplyWienWebApplicationConfiguration(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseCors("AllowFrontend");

        app.UseAuthorization();

        app.MapPartnerEndpoints();
        app.MapPolicyEndpoints();

        app.MapHub<PartnerHub>("/hubs/partners");

        return app;
    }
}
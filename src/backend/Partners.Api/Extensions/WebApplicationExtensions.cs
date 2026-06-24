using Partners.Api.Hubs;

namespace Partners.Api.Extensions;

internal static class WebApplicationExtensions
{
    public static WebApplication ApplyWienWebApplicationConfiguration(this WebApplication app)
    {
        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseCors("AllowFrontend");

        app.UseAuthorization();

        app.MapControllers();

        app.MapHub<PartnerHub>("/hubs/partners");

        return app;
    }
}
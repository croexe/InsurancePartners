using Partners.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.ApplyWienConfiguration();

var app = builder.Build();

await app.ApplyWienMigrationsAndSeedAsync();

app.ApplyWienWebApplicationConfiguration();
app.Run();

public partial class Program { }

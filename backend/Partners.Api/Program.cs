using Partners.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.ApplyWienConfiguration();

var app = builder.Build();

app.ApplyWienWebApplicationConfiguration();
app.Run();

using Partners.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.ApplyWienConfiguration(builder.Environment, builder.Configuration);

var app = builder.Build();
app.ApplyWienWebApplicationConfiguration();
app.Run();
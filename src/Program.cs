var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel((context, options) => options.ListenAnyIP(5001, options => options.UseHttps(context.Configuration.GetValue<string>("TLS_PATH"), context.Configuration.GetValue<string?>("TLS_PASSWORD"))));

var app = builder.Build();

app.MapGet("/", (IConfiguration configuration) => $"Hello, {configuration.GetValue<string>("NAME", "World")}!");

app.Run();

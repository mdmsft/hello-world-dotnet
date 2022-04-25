var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => options.ListenAnyIP(5001, options => options.UseHttps("./tls.pfx")));

var app = builder.Build();

app.MapGet("/", () => $"Hello, {Environment.GetEnvironmentVariable("NAME") ?? "World"}!");

app.Run();

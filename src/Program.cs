var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseKestrel((context, options) => options.ListenAnyIP(5001, options => options.UseHttps(context.Configuration.GetValue<string>("TLS_PATH"), context.Configuration.GetValue<string?>("TLS_PASSWORD"))));
builder.Services.AddHealthChecks();

var app = builder.Build();
app.UseHealthChecks("/healthz");
app.MapGet("/", (HttpRequest request, IConfiguration configuration) => $"Hello {configuration.GetValue<string>("NAME", "World")} from {Environment.MachineName} via {request.Protocol}");
app.Run();

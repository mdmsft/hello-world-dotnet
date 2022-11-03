var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseKestrel((context, options) => options.ListenAnyIP(5001, options =>
{
    string? tlsPath = context.Configuration.GetValue<string?>("TLS_PATH");
    string? tlsPassword = context.Configuration.GetValue<string?>("TLS_PASSWORD");
    if (tlsPath is { Length: > 0 }  && tlsPassword is { Length: > 0 })
    {
        options.UseHttps(tlsPath, tlsPassword);
    }
}));
builder.Services.AddHealthChecks();

var app = builder.Build();
app.UseHealthChecks("/healthz");
app.MapGet("/", (HttpRequest request, IConfiguration configuration, ILogger<Program> logger) =>
{
    string name = configuration.GetValue<string>("NAME", "World");
    string machine = Environment.MachineName;
    string origin = request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "N/A";
    string protocol = request.Protocol;
    logger.LogInformation(1000, "Name: {Name}, Machine: {Machine}, Origin: {Origin}, Protocol: {Protocol}", name, machine, origin, protocol);
    return $"Hello {name} from {machine} and origin {origin} via {protocol}";
});
app.Run();

using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseKestrel((context, options) => options.ListenAnyIP(5001, options =>
{
    string? pfxPath = context.Configuration.GetValue<string?>("PFX_PATH");
    string? pfxPassword = context.Configuration.GetValue<string?>("PFX_PASSWORD");
    string? pemCrtPath = context.Configuration.GetValue<string?>("PEM_CRT_PATH");
    string? pemKeyPath = context.Configuration.GetValue<string?>("PEM_KEY_PATH");
    string? pemPassword = context.Configuration.GetValue<string?>("PEM_PASSWORD");

    if (pfxPath is { Length: > 0 })
    {
        options.UseHttps(pfxPath, pfxPassword);
    }

    else if (pemCrtPath is { Length: > 0 })
    {
        X509Certificate2 certificate = pemPassword is { Length: > 0 } ?
            X509Certificate2.CreateFromEncryptedPemFile(pemCrtPath, pemPassword, pemKeyPath) :
            X509Certificate2.CreateFromPemFile(pemCrtPath, pemKeyPath);
        options.UseHttps(certificate);
    }
}));
builder.Services.AddHealthChecks();

var app = builder.Build();
app.UseHealthChecks("/healthz");
app.MapGet("/", (HttpRequest request, IConfiguration configuration, ILogger<Program> logger) =>
{
    string name = configuration.GetValue<string>("NAME") ?? "World";
    string machine = Environment.MachineName;
    string origin = request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "N/A";
    string protocol = request.Protocol;
    logger.LogInformation(1000, "Name: {Name}, Machine: {Machine}, Origin: {Origin}, Protocol: {Protocol}", name, machine, origin, protocol);
    return $"Hello {name} from {machine} and origin {origin} via {protocol}";
});
app.Run();

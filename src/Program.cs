using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseKestrel((context, options) => options.ListenAnyIP(5001, options => UseHttps(context, options)));
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

static void UseHttps(WebHostBuilderContext context, ListenOptions options)
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
        if (pemKeyPath is { Length: > 0 })
        {
            var rsa = RSA.Create();
            using StreamReader reader = File.OpenText(pemKeyPath);
            rsa.ImportFromPem(reader.ReadToEnd());
            string subjectPublicKey = Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo());

            X509Certificate2Collection certificateCollection = new();
            certificateCollection.ImportFromPemFile(pemCrtPath);
            foreach (var certificate in certificateCollection)
            {
                string certificateSubjectPublicKey = Convert.ToBase64String(certificate.PublicKey.ExportSubjectPublicKeyInfo());
                if (string.Equals(subjectPublicKey, certificateSubjectPublicKey, StringComparison.Ordinal))
                {
                    options.UseHttps(certificate.CopyWithPrivateKey(rsa));
                    return;
                }
            }
            Console.WriteLine("The provided key does not match the public key for this certificate");
            Environment.Exit(-1);
        }

        else
        {
            X509Certificate2 certificate = pemPassword is { Length: > 0 } ?
                X509Certificate2.CreateFromEncryptedPemFile(pemCrtPath, pemPassword) :
                X509Certificate2.CreateFromPemFile(pemCrtPath);
        
            options.UseHttps(certificate);
        }
    }
}
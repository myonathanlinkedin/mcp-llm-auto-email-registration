using MCPServer.Tools;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Build configuration and read from appsettings.json
var config = BuildConfiguration();
string serverName = config["MCP:ServerName"];  // Read MCP server name from config

// Register services
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<APITools>();

// Add OpenTelemetry with logging, tracing, and metrics
ResourceBuilder resource = ResourceBuilder.CreateDefault().AddService(serverName);

builder.Services.AddOpenTelemetry()
    .WithTracing(b => b.AddSource("*")
                       .AddAspNetCoreInstrumentation()
                       .AddHttpClientInstrumentation()
                       .SetResourceBuilder(resource))
    .WithMetrics(b => b.AddMeter("*")
                       .AddAspNetCoreInstrumentation()
                       .AddHttpClientInstrumentation()
                       .SetResourceBuilder(resource))
    .WithLogging(b => b.SetResourceBuilder(resource))
    .UseOtlpExporter();


// Add logging to console with trace-level logging
builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

// Build and run the application
var app = builder.Build();

app.MapMcp();

app.Run();

// Method to build configuration from appsettings.json
static IConfiguration BuildConfiguration()
{
    return new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory()) // Ensure base path is set
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();
}

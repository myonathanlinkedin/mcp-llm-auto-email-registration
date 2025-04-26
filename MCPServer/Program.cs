using MCPServer.Tools;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Build configuration and read from appsettings.json
var config = BuildConfiguration();
string serverName = config["MCP:ServerName"];  // Read MCP server name from config

// Configure Serilog for logging (Console and File)
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()  // Set the minimum log level to Debug or other levels as needed
    .WriteTo.Console(Serilog.Events.LogEventLevel.Information)  // Log to console with information level
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day,
        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)  // Log to file with rolling interval
    .CreateLogger();

// Clear existing log providers and add Serilog to the pipeline using the SerilogLoggerProvider
builder.Logging.ClearProviders();
builder.Logging.AddProvider(new SerilogLoggerProvider(Log.Logger));  // Use SerilogLoggerProvider explicitly

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
    .WithLogging(b => b.SetResourceBuilder(resource))  // OpenTelemetry's internal logging configuration
    .UseOtlpExporter();  // Export telemetry data to OTLP

// Build and run the application
var app = builder.Build();

app.MapMcp();

app.Run();

// Gracefully close and flush logs when the application shuts down
Log.CloseAndFlush();

// Method to build configuration from appsettings.json
static IConfiguration BuildConfiguration()
{
    return new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory()) // Ensure base path is set
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();
}

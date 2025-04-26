using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure services  
builder.Services
   .AddIdentityApplication(builder.Configuration)
   .AddIdentityInfrastructure(builder.Configuration)
   .AddIdentityWebComponents()
   .AddTokenAuthentication(builder.Configuration)
   .AddEventSourcing()
   .AddModelBinders()
   .AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "Web API", Version = "v1" }))
   .AddHttpClient()
   .AddMcpClient(builder.Configuration);

// Configure logging  
Log.Logger = new LoggerConfiguration()
   .WriteTo.Console()  // Log to console  
   .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)  // Log to file with rolling interval  
   .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();  // Use Serilog for logging  

// Build and configure the app  
var app = builder.Build();

app
   .UseHttpsRedirection()
   .UseWebService(app.Environment)
   .Initialize();

app.Run();

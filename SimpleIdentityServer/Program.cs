var builder = WebApplication.CreateBuilder(args);

builder
    .Services
    .AddIdentityApplication(builder.Configuration)
    .AddIdentityInfrastructure(builder.Configuration)
    .AddIdentityWebComponents();

builder.Services
    .AddTokenAuthentication(builder.Configuration)
    .AddEventSourcing()
    .AddModelBinders()
    .AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new() { Title = "Web API", Version = "v1" });
    })
    .AddHttpClient();

var app = builder.Build();

app
    .UseHttpsRedirection()  
    .UseWebService(app.Environment)
    .Initialize();

app.Run();
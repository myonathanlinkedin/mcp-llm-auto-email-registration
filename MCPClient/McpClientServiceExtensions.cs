using MCPClient.MCPClientServices;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;
using OpenAI;
using OpenTelemetry.Logs;
using System.ClientModel;

public static class McpClientServiceExtensions
{
    public static IServiceCollection AddMcpClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ILoggerFactory>(sp =>
            LoggerFactory.Create(builder =>
                builder.AddOpenTelemetry(opt => opt.AddOtlpExporter())));

        services.AddScoped(sp =>
        {
            var endpoint = configuration["API:Endpoint"]!;
            var apiKey = configuration["API:ApiKey"]!;
            return new OpenAIClient(
                new ApiKeyCredential(apiKey),
                new OpenAIClientOptions { Endpoint = new Uri(endpoint) });
        });

        services.AddScoped<ISamplingChatClient>(sp =>
        {
            var openAIClient = sp.GetRequiredService<OpenAIClient>();
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            return openAIClient.GetChatClient("gpt-4o-mini")
                .AsIChatClient()
                .AsBuilder()
                .UseOpenTelemetry(loggerFactory: loggerFactory, configure: o => o.EnableSensitiveData = true)
                .Build() as ISamplingChatClient
                ?? throw new InvalidCastException("SamplingChatClient build failed.");
        });

        services.AddScoped<IChatClient>(sp =>
        {
            var openAIClient = sp.GetRequiredService<OpenAIClient>();
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            return openAIClient.GetChatClient("gpt-4o-mini")
                .AsIChatClient()
                .AsBuilder()
                .UseFunctionInvocation()
                .UseOpenTelemetry(loggerFactory: loggerFactory, configure: o => o.EnableSensitiveData = true)
                .Build();
        });

        services.AddScoped(sp =>
        {
            var serverEndpoint = configuration["MCP:Endpoint"]!;
            var serverName = configuration["MCP:ServerName"]!;
            return new SseClientTransport(new SseClientTransportOptions
            {
                Endpoint = new Uri($"{serverEndpoint}/sse"),
                Name = serverName,
                ConnectionTimeout = TimeSpan.FromMinutes(1)
            });
        });

        services.AddScoped<IMcpClient>(sp =>
        {
            var transport = sp.GetRequiredService<SseClientTransport>();
            var samplingClient = sp.GetRequiredService<ISamplingChatClient>();
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

            return McpClientFactory.CreateAsync(
                transport,
                clientOptions: new()
                {
                    Capabilities = new()
                    {
                        Sampling = new() { SamplingHandler = samplingClient.CreateSamplingHandler() }
                    }
                },
                loggerFactory: loggerFactory).GetAwaiter().GetResult();
        });

        // Call ListToolsAsync and register the tools as a singleton
        services.AddSingleton<IEnumerable<McpClientTool>>(sp =>
        {
            var mcpClient = sp.GetRequiredService<IMcpClient>();
            var tools = mcpClient.ListToolsAsync().GetAwaiter().GetResult();
            return tools;
        });

        services.AddScoped<IMCPServerRequester, MCPServerRequester>();

        return services;
    }
}

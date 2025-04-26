using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;
using OpenAI;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.ClientModel;

class Program
{
    static async Task Main(string[] args)
    {
        var config = BuildConfiguration();
        var loggerFactory = SetupLogging();

        Console.WriteLine($"Connecting client to MCP '{config["MCP:ServerName"]}' server");

        var openAIClient = CreateOpenAIClient(config["API:Endpoint"], config["API:ApiKey"]);
        var samplingClient = CreateSamplingClient(openAIClient, loggerFactory);
        var transport = CreateSseTransport(config["MCP:Endpoint"], config["MCP:ServerName"]);

        var mcpClient = await CreateMcpClientAsync(transport, samplingClient, loggerFactory);
        await DisplayToolsAsync(mcpClient);
        await RunChatLoopAsync(CreateChatClient(openAIClient, loggerFactory), await mcpClient.ListToolsAsync());
    }

    static IConfiguration BuildConfiguration() =>
        new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

    static ILoggerFactory SetupLogging() =>
        LoggerFactory.Create(builder =>
            builder.AddOpenTelemetry(opt => opt.AddOtlpExporter()));

    static OpenAIClient CreateOpenAIClient(string apiEndpoint, string apiKey) =>
        new OpenAIClient(new ApiKeyCredential(apiKey), new OpenAIClientOptions { Endpoint = new Uri(apiEndpoint) });

    static IChatClient CreateSamplingClient(OpenAIClient client, ILoggerFactory loggerFactory) =>
        client.GetChatClient("gpt-4o-mini")
              .AsIChatClient()
              .AsBuilder()
              .UseOpenTelemetry(loggerFactory: loggerFactory, configure: o => o.EnableSensitiveData = true)
              .Build();

    static IChatClient CreateChatClient(OpenAIClient client, ILoggerFactory loggerFactory) =>
        client.GetChatClient("gpt-4o-mini")
              .AsIChatClient()
              .AsBuilder()
              .UseFunctionInvocation()
              .UseOpenTelemetry(loggerFactory: loggerFactory, configure: o => o.EnableSensitiveData = true)
              .Build();

    static SseClientTransport CreateSseTransport(string serverEndpoint, string serverName) =>
        new SseClientTransport(new SseClientTransportOptions
        {
            Endpoint = new Uri($"{serverEndpoint}/sse"),
            Name = serverName,
            ConnectionTimeout = TimeSpan.FromMinutes(1)
        });

    static async Task<IMcpClient> CreateMcpClientAsync(SseClientTransport transport, IChatClient samplingClient, ILoggerFactory loggerFactory) =>
        await McpClientFactory.CreateAsync(
            transport,
            clientOptions: new()
            {
                Capabilities = new()
                {
                    Sampling = new() { SamplingHandler = samplingClient.CreateSamplingHandler() }
                }
            },
            loggerFactory: loggerFactory);

    static async Task DisplayToolsAsync(IMcpClient mcpClient)
    {
        Console.WriteLine("\nTools available:");
        var tools = await mcpClient.ListToolsAsync();
        foreach (var tool in tools)
            Console.WriteLine($"  {tool}");
    }

    static async Task RunChatLoopAsync(IChatClient chatClient, IEnumerable<McpClientTool> tools)
    {
        var messages = new List<ChatMessage>();

        while (true)
        {
            Console.Write("Q: ");
            string? input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input)) continue;

            messages.Add(new(ChatRole.User, input));

            var results = chatClient.GetStreamingResponseAsync(messages, new() { Tools = [.. tools] });
            await foreach (var update in results)
            {
                Console.Write(update);
            }

            Console.WriteLine("\n");
        }
    }
}

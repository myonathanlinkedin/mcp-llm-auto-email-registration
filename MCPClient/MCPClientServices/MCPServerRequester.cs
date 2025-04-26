using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using System.Text;
using Microsoft.Extensions.Logging;

namespace MCPClient.MCPClientServices
{
    public class MCPServerRequester : IMCPServerRequester
    {
        private readonly IChatClient chatClient;
        private readonly IEnumerable<McpClientTool> tools;
        private readonly ILogger<MCPServerRequester> logger;

        public MCPServerRequester(IChatClient chatClient, IEnumerable<McpClientTool> tools, ILogger<MCPServerRequester> logger)
        {
            this.chatClient = chatClient;
            this.tools = tools;
            this.logger = logger;
        }

        public async Task<Result<string>> RequestAsync(string prompt)
        {
            try
            {
                var messages = new List<ChatMessage>
                {
                    new ChatMessage(ChatRole.User, prompt)
                };

                var results = chatClient.GetStreamingResponseAsync(messages, new() { Tools = tools.Cast<AITool>().ToList() });

                StringBuilder responseBuilder = new StringBuilder();

                await foreach (var update in results)
                {
                    responseBuilder.Append(update);
                }

                return Result<string>.SuccessWith(responseBuilder.ToString());
            }
            catch (Exception ex)
            {
                // Log the exception using injected logger
                logger.LogError(ex, "An error occurred while processing the request.");

                // Return a failure result with the error message
                return Result<string>.Failure(new List<string> { $"An error occurred: {ex.Message}" });
            }
        }
    }
}

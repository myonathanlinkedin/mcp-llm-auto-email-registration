using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using System.Text;

namespace MCPClient.MCPClientServices
{
    public class MCPServerRequester(IChatClient chatClient, IEnumerable<McpClientTool> tools) : IMCPServerRequester
    {
        public async Task<string> RequestAsync(string prompt)
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

            return responseBuilder.ToString();
        }
    }
}

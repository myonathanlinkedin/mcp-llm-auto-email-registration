using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace MCPServer.Tools
{
    [McpServerToolType]
    public sealed class APITools
    {
        // Existing method
        [McpServerTool, Description("Get echo with random seed")]
        public static async Task<string> GetEchoRandomSeed([Description("Echo from the server with random seed")] string paramInput)
        {
            // Generate a random string to use as a seed
            Random random = new Random();
            int randomSeed = random.Next();
            string seedString = randomSeed.ToString(CultureInfo.InvariantCulture);

            // Hash the random seed using SHA-256
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(seedString));

                // Convert the hash bytes to a hex string
                string hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

                // Return the formatted string with Echo and the hashed seed
                return await Task.FromResult($"Echo MCPServer: {paramInput}, here is the seed: {hashString}");
            }
        }
    }
}

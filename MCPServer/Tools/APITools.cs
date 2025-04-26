using ModelContextProtocol.Server;
using Serilog;
using System.ComponentModel;
using System.Net.Http.Headers;

namespace MCPServer.Tools
{
    [McpServerToolType]
    public sealed class APITools
    {
        private const string JsonMediaType = "application/json";
        private const string RegisterDescription = "Register a user account. An email will be sent upon successful registration.";

        private readonly IHttpClientFactory _httpClientFactory;

        public APITools(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [McpServerTool, Description(RegisterDescription)]
        public async Task<string> RegisterAsync([Description("Email address to register")] string email)
        {
            var password = GenerateRandomPassword(6);
            var payload = new
            {
                email,
                password,
                confirmPassword = password
            };

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri("https://localhost:7190");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(JsonMediaType));

                var response = await client.PostAsJsonAsync("/api/Identity/Register/Register", payload);

                if (response.IsSuccessStatusCode)
                {
                    Log.Information("Successfully registered user: {Email}", email);
                    return "An email has been sent. Please check your inbox to complete registration.";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Log.Error("Failed to register user: {Email}, StatusCode: {StatusCode}, Error: {Error}",
                        email, response.StatusCode, errorContent);
                    return $"Failed to register user. StatusCode: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An exception occurred while registering user: {Email}", email);
                return "An error occurred during registration.";
            }
        }

        private static string GenerateRandomPassword(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var password = new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            return password;
        }
    }
}

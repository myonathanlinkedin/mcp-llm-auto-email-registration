using Microsoft.Extensions.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCPClient.MCPClientServices
{
    public interface IMCPServerRequester
    {
        Task<Result<string>> RequestAsync(string prompt, ChatRole? chatRole = null, bool useSession = true);
    }
}

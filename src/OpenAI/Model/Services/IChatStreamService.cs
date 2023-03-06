using System;
using System.Threading.Tasks;

namespace OpenAI;

public interface IChatStreamService
{
    Task GetResponseDataAsync(ChatServiceSettings settings, Action<string> fetchResponse);
}

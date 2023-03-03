using System.Threading.Tasks;

namespace OpenAI;

public interface IChatService
{
    Task<ChatResponse?> GetResponseDataAsync(ChatServiceSettings settings);
}

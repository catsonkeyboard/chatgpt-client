using System.Threading.Tasks;

namespace OpenAI;

public interface ICompletionsService
{
    Task<CompletionsResponse?> GetResponseDataAsync(CompletionsServiceSettings settings);
}

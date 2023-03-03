using System.Text.Json.Serialization;

namespace OpenAI;

public class ChatResponseError : ChatResponse
{
    [JsonPropertyName("error")]
    public ChatError? Error { get; set; }
}

using System.Text.Json.Serialization;

namespace OpenAI;

public class StreamChatMessage
{
    [JsonPropertyName("content")]
    public string? Content { get; set; }
}

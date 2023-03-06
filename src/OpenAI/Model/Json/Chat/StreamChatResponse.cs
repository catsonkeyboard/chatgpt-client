using System.Text.Json.Serialization;

namespace OpenAI;

public class StreamChatResponse : ChatResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("object")]
    public string? Object { get; set; }

    [JsonPropertyName("created")]
    public int Created { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("choices")]
    public StreamChatChoice[]? Choices { get; set; }
}

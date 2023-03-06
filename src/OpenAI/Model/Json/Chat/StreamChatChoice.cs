using System.Text.Json.Serialization;

namespace OpenAI;

public class StreamChatChoice
{

    [JsonPropertyName("delta")]
    public StreamChatMessage? delta { get; set; }

    [JsonPropertyName("index")]
    public int? Index { get; set; }

    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }

}

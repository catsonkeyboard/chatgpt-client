using System.Text.Json.Serialization;

namespace OpenAI;

[JsonSerializable(typeof(ChatRequestBody))]
[JsonSerializable(typeof(ChatResponseSuccess))]
[JsonSerializable(typeof(ChatChoice))]
[JsonSerializable(typeof(ChatMessage))]
[JsonSerializable(typeof(ChatUsage))]
[JsonSerializable(typeof(ChatResponseError))]
[JsonSerializable(typeof(ChatError))]
[JsonSerializable(typeof(StreamChatResponse))]
[JsonSerializable(typeof(StreamChatChoice))]
[JsonSerializable(typeof(StreamChatMessage))]
public partial class ChatJsonContext : JsonSerializerContext
{
}

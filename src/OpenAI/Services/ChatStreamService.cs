using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace OpenAI;

public class ChatStreamService : IChatStreamService
{
    //private static HttpClientHandler handler = new HttpClientHandler
    //{
    //    Proxy = new WebProxy("http://localhost:7890")
    //};

    private static readonly ChatJsonContext s_serializerContext = new(
        new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            IgnoreReadOnlyProperties = true
        });

    private static string GetRequestBodyJson(ChatServiceSettings settings)
    {
        // Set up the request body
        var requestBody = new ChatRequestBody
        {
            Model = settings.Model,
            Messages = settings.Messages,
            MaxTokens = settings.MaxTokens,
            Temperature = settings.Temperature,
            TopP = settings.TopP,
            N = 1,
            Stream = true,
            Stop = settings.Stop,
            FrequencyPenalty = 0.0m,
            PresencePenalty = 0.0m,
            User = null
        };

        // Serialize the request body to JSON using the JsonSerializer.
        return JsonSerializer.Serialize(requestBody, s_serializerContext.ChatRequestBody);
    }

    private static async Task SendApiRequestAsync(string apiUrl, string apiKey, string requestBodyJson,Action<string> fetchResponse)
    {
        using HttpClient s_client = new();
        // Create a new HttpClient for making the API request
        // Set the API key in the request headers
        if (s_client.DefaultRequestHeaders.Contains("Authorization"))
        {
            s_client.DefaultRequestHeaders.Remove("Authorization");
        }
        s_client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        s_client.Timeout = TimeSpan.FromMilliseconds(-1); // 设置超时时间为无限
        s_client.DefaultRequestHeaders
              .Accept
              .Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
        using var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
        request.Headers.Add("Connection", "keep-alive");
        request.Content = new StringContent(requestBodyJson, Encoding.UTF8, "application/json");
        using var response = await s_client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode(); // 确保响应成功
        var sseStream = await response.Content.ReadAsStreamAsync(); // 获取 SSE 数据流

        // 处理 SSE 响应
        using (var stream = await response.Content.ReadAsStreamAsync())
        using (var reader = new StreamReader(stream))
        {
            while (!reader.EndOfStream)
            {
                string line = await reader.ReadLineAsync();

                if (line == null) continue;
                if (line.StartsWith(":")) continue; //注释行

                // 每个事件分为多行，并以空行分隔
                string[] data = line.Split(new[] { ':' }, 2);

                if (data.Length == 2)
                {
                    string eventName = data[0];
                    string eventData = data[1].Substring(1);
                    if ("[DONE]".Equals(eventData))
                    {
                        fetchResponse.Invoke("[DONE]");
                    }
                    else
                    {
                        var responseBody = JsonSerializer.Deserialize(eventData, s_serializerContext.StreamChatResponse);
                        if (responseBody != null && responseBody.Choices[0].delta?.Content != null)
                        {
                            await System.Windows.Application.Current?.Dispatcher.
                                InvokeAsync(() => {
                                    fetchResponse.Invoke(responseBody.Choices[0].delta.Content);
                                }, DispatcherPriority.ContextIdle);
                        }
                    }
                }
            }
        }
        response.Dispose();
        //// 读取 SSE 数据流，并根据特定规则解析数据
        //var buffer = new byte[4096];
        //while (await sseStream.ReadAsync(buffer, 0, buffer.Length) > 0)
        //{
        //    var message = System.Text.Encoding.UTF8.GetString(buffer, 0, Array.IndexOf(buffer, (byte)0));
        //    var eventFieldIndex = message.IndexOf("data:");
        //    if (eventFieldIndex > -1)
        //    {
        //        var eventData = message.Substring(eventFieldIndex + 6);
        //        var responseBody = JsonSerializer.Deserialize(eventData, s_serializerContext.ChatResponseSuccess);
        //        fetchResponse.Invoke(responseBody);
        //    }
        //}
    }

    public async Task GetResponseDataAsync(ChatServiceSettings settings,Action<string> fetchResponse)
    {
        // Set up the API URL and API key
        var apiUrl = "https://api.openai.com/v1/chat/completions";
        //var apiKey = Environment.GetEnvironmentVariable(Constants.EnvironmentVariableApiKey);
        //if (apiKey is null)
        //{
        //    return null;
        //}

        var apiKey = settings.ApiKey;

        // Get the request body JSON
        var requestBodyJson = GetRequestBodyJson(settings);

        // Send the API request and get the response data
        await SendApiRequestAsync(apiUrl, apiKey, requestBodyJson, fetchResponse);
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;

namespace ChatWpfUI
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private string _applicationTitle = "ChatGPT-WPF-UI";

        [ObservableProperty]
        private string _sendMessage = string.Empty;

        [ObservableProperty]
        private string _messageContent = string.Empty;

        public MainViewModel(IServiceProvider serviceProvider) 
        {
            _serviceProvider = serviceProvider;
        }

        [RelayCommand]
        public void Send() 
        {
            var chatMessages = new List<ChatMessage>();
            chatMessages.Add(new ChatMessage { Role = "user", Content = SendMessage });
            var chatServiceSettings = new ChatServiceSettings
            {
                Model = "gpt-3.5-turbo",
                Messages = chatMessages.ToArray(),
                Suffix = null,
                Temperature = 0.7m,
                MaxTokens = 256,
                TopP = 1.0m,
                Stop = null,
                ApiKey = ""
            };

            IChatService chatService = _serviceProvider.GetService<IChatService>();
            var chat = chatService.GetResponseDataAsync(chatServiceSettings);
            chat.GetAwaiter().OnCompleted(() =>
            {
                ChatResponse result = chat.Result;
                if (result != null && result is ChatResponseSuccess success)
                {
                    foreach (var choice in success.Choices)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            MessageContent = choice.Message.Content;
                        });
                    }
                }
            });
        }
    }
}

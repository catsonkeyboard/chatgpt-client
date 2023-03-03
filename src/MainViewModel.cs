using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private string _textMessage = string.Empty;

        [ObservableProperty]
        private string _messageContent = string.Empty;

        [ObservableProperty]
        private ObservableCollection<ChatHistoryViewModel> _chatList;

        [ObservableProperty]
        private ChatHistoryViewModel _selectedChat;

        [ObservableProperty]
        private ObservableCollection<ChatViewModel> _chatMessageList;

        public MainViewModel(IServiceProvider serviceProvider) 
        {
            _serviceProvider = serviceProvider;
            ChatList = new ObservableCollection<ChatHistoryViewModel> 
            {
                new ChatHistoryViewModel { Name = "111" },
                new ChatHistoryViewModel { Name = "222" },
                new ChatHistoryViewModel { Name = "333" },
                new ChatHistoryViewModel { Name = "444" },
            };

            ChatMessageList = new ObservableCollection<ChatViewModel>
            {
                new ChatViewModel { IsUser = true, Message = "Life is full of twists and turns. Sometimes we go through tough times that test our strength and resilience. But it's important to remember that every challenge we face is an opportunity for growth and learning. We can't control what happens to us, but we can control how we respond. We can choose to let difficult experiences break us down or build us up. We can choose to dwell on the negative or look for the positive. We can choose to give up or keep moving forward. Ultimately, our attitudes and actions determine our destiny. So, let's choose to be strong, courageous, and optimistic - no matter what life throws our way. Let's believe in ourselves, trust our instincts, and never give up on our dreams. Let's focus on the things we can control and let go of the things we can't. Let's surround ourselves with people who lift us up and inspire us to be our best selves. Let's be kind, compassionate, and understanding towards ourselves and others. Let's take life one day at a time, and make the most of every moment. After all, life is short, but it can be filled with joy, love, and purpose if we make it so. So, let's live each day to the fullest, and make a positive difference in the world around us.", Time = DateTime.Now.AddDays(1) },
                new ChatViewModel { IsUser = false, Message = "Life is full of twists and turns. Sometimes we go through tough times that test our strength and resilience. But it's important to remember that every challenge we face is an opportunity for growth and learning. We can't control what happens to us, but we can control how we respond. We can choose to let difficult experiences break us down or build us up. We can choose to dwell on the negative or look for the positive. We can choose to give up or keep moving forward. Ultimately, our attitudes and actions determine our destiny. So, let's choose to be strong, courageous, and optimistic - no matter what life throws our way. Let's believe in ourselves, trust our instincts, and never give up on our dreams. Let's focus on the things we can control and let go of the things we can't. Let's surround ourselves with people who lift us up and inspire us to be our best selves. Let's be kind, compassionate, and understanding towards ourselves and others. Let's take life one day at a time, and make the most of every moment. After all, life is short, but it can be filled with joy, love, and purpose if we make it so. So, let's live each day to the fullest, and make a positive difference in the world around us.", Time = DateTime.Now.AddDays(2) },
                new ChatViewModel { IsUser = false, Message = "Life is full of twists and turns. Sometimes we go through tough times that test our strength and resilience. But it's important to remember that every challenge we face is an opportunity for growth and learning. We can't control what happens to us, but we can control how we respond. We can choose to let difficult experiences break us down or build us up. We can choose to dwell on the negative or look for the positive. We can choose to give up or keep moving forward. Ultimately, our attitudes and actions determine our destiny. So, let's choose to be strong, courageous, and optimistic - no matter what life throws our way. Let's believe in ourselves, trust our instincts, and never give up on our dreams. Let's focus on the things we can control and let go of the things we can't. Let's surround ourselves with people who lift us up and inspire us to be our best selves. Let's be kind, compassionate, and understanding towards ourselves and others. Let's take life one day at a time, and make the most of every moment. After all, life is short, but it can be filled with joy, love, and purpose if we make it so. So, let's live each day to the fullest, and make a positive difference in the world around us.", Time = DateTime.Now.AddDays(1) },
            };
        }

        [RelayCommand]
        public void SendMessage()
        {
            var chatMessages = new List<ChatMessage>();
            chatMessages.Add(new ChatMessage { Role = "user", Content = TextMessage });
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

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
        private string _applicationTitle = "ChatGPT-UI";

        [ObservableProperty]
        private ChatMessageViewModel _prompt = new ChatMessageViewModel();

        [ObservableProperty]
        private ObservableCollection<ChatHistoryViewModel> _chatList;

        [ObservableProperty]
        private ChatHistoryViewModel _selectedChat;

        [ObservableProperty]
        private ObservableCollection<ChatMessageViewModel> _chatMessageList;

        [ObservableProperty]
        private ChatMessageViewModel _currentChatMessage;

        [ObservableProperty]
        private bool _isEnabled;

        public MainViewModel(IServiceProvider serviceProvider) 
        {
            _serviceProvider = serviceProvider;
            ChatList = new ObservableCollection<ChatHistoryViewModel>();
            ChatMessageList = new ObservableCollection<ChatMessageViewModel>();
            NewChat();
        }

        private static ChatMessage[] CreateChatPrompt(
            ChatMessageViewModel sendMessage,
            ObservableCollection<ChatMessageViewModel> messages)
        {
            var chatMessages = new List<ChatMessage>();
            chatMessages.Add(new ChatMessage
            {
                Role = "system",
                Content = ""
            });
            foreach (var message in messages)
            {
                if (!string.IsNullOrEmpty(message.Message) && message.Result is { })
                {
                    chatMessages.Add(new ChatMessage
                    {
                        Role = "user",
                        Content = message.Message
                    });
                    chatMessages.Add(new ChatMessage
                    {
                        Role = "assistant",
                        Content = message.Result.Message
                    });
                }
            }

            chatMessages.Add(new ChatMessage
            {
                Role = "user",
                Content = sendMessage.Prompt
            });

            return chatMessages.ToArray();
        }

        private void NewChat()
        {
            var newChat = new ChatHistoryViewModel
            {
                Name = "New Chat",
            };
            ChatList.Add(newChat);

            Prompt = new ChatMessageViewModel
            {
                Prompt = string.Empty,
                Send = Send
            };
        }

        private async Task NewAction()
        {
            NewChat();
            await Task.Yield();
        }


        public async Task Send(ChatMessageViewModel sendMessage)
        {
            if(string.IsNullOrEmpty(sendMessage.Prompt))
            {
                return;
            }

            var chatPrompts = CreateChatPrompt(sendMessage, ChatMessageList);
            IsEnabled = false;
            sendMessage.IsUser = true;
            sendMessage.IsSent = true;
            ChatMessageViewModel? promptMessage;
            ChatMessageViewModel? resultMessage = null;
            if (sendMessage.Result is { })
            {
                promptMessage = sendMessage;
                resultMessage = sendMessage.Result;
            }
            else
            {
                promptMessage = new ChatMessageViewModel();
                promptMessage.Send = this.Send;
                ChatMessageList.Add(promptMessage);
            }

            var prompt = sendMessage.Prompt;
            promptMessage.Message = prompt;
            promptMessage.Prompt = "";
            promptMessage.IsSent = true;
            promptMessage.IsUser = true;
            promptMessage.Time = DateTime.Now;

            CurrentChatMessage = promptMessage;
            var chatServiceSettings = new ChatServiceSettings
            {
                Model = "gpt-3.5-turbo",
                Messages = chatPrompts,
                Suffix = null,
                Temperature = 0.7m,
                MaxTokens = 256,
                TopP = 1.0m,
                Stop = null,
                ApiKey = ""
            };
            var responseStr = default(string);
            bool isResponseStrError = false;
            IChatService chatService = _serviceProvider.GetService<IChatService>();
            var responseData = await chatService.GetResponseDataAsync(chatServiceSettings);
            if(responseData is null)
            {
                responseStr = "error";
                isResponseStrError = true;
            }
            else if (responseData is ChatResponseError error)
            {
                var message = error.Error?.Message;
                responseStr = message ?? "Unknown error.";
                isResponseStrError = true;
            }
            else if (responseData is ChatResponseSuccess success)
            {
                var message = success.Choices?.FirstOrDefault()?.Message?.Content?.Trim();
                responseStr = message ?? "";
            }

            // Update

            if (isResponseStrError)
            {
                resultMessage = promptMessage;
            }

            if (resultMessage is null)
            {
                resultMessage = new ChatMessageViewModel
                {
                    IsSent = false,
                };
                resultMessage.Send = this.Send;
                ChatMessageList.Add(resultMessage);
            }
            else
            {
                if (!isResponseStrError)
                {
                    resultMessage.IsSent = true;
                }
            }
            resultMessage.Message = responseStr;
            resultMessage.IsUser = false;
            resultMessage.Time = DateTime.Now;
            resultMessage.IsError = isResponseStrError;
            resultMessage.Prompt = isResponseStrError ? prompt : "";

            if (ChatMessageList.LastOrDefault() == resultMessage)
            {
                resultMessage.IsSent = false;
            }

            //set resultMessage to current
            CurrentChatMessage = resultMessage;
            //set resultMessage to promptMessage's result
            promptMessage.Result = isResponseStrError ? null : resultMessage;

            IsEnabled = true;
        }
    }
}

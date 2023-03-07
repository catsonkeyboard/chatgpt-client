using ChatWpfUI.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace ChatWpfUI
{
    public partial class MainViewModel : ObservableObject, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private string _applicationTitle = "ChatGPT-UI";

        [ObservableProperty]
        private ChatMessageViewModel _prompt = new ChatMessageViewModel();

        [ObservableProperty]
        private ObservableCollection<ChatViewModel> _chatList;

        [ObservableProperty]
        private ChatViewModel _selectedChat;

        [ObservableProperty]
        private ObservableCollection<ChatMessageViewModel> _chatMessageList;

        [ObservableProperty]
        private ChatMessageViewModel _currentChatMessage;

        [ObservableProperty]
        private bool _isEnabled;

        public MainViewModel(IServiceProvider serviceProvider) 
        {
            _serviceProvider = serviceProvider;
            ChatList = new ObservableCollection<ChatViewModel>();
            ChatMessageList = new ObservableCollection<ChatMessageViewModel>();
            LoadHistory();
            NewChat();
            this.PropertyChanged += MainViewModel_PropertyChanged;
            this.MainViewModel_PropertyChanged(null, new PropertyChangedEventArgs(nameof(SelectedChat)));
        }

        private void MainViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(SelectedChat))
            {
                if(SelectedChat.ChatMessageList != null)
                {
                    ChatMessageList = new ObservableCollection<ChatMessageViewModel>(SelectedChat.ChatMessageList);
                }
                else
                {
                    ChatMessageList = new ObservableCollection<ChatMessageViewModel>();
                }
            }
        }

        private static ChatMessage[] CreateChatPrompt(
            ChatMessageViewModel sendMessage,
            ObservableCollection<ChatMessageViewModel> messages)
        {
            var chatMessages = new List<ChatMessage>();
            //chatMessages.Add(new ChatMessage
            //{
            //    Role = "system",
            //    Content = ""
            //});
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
            var newChat = new ChatViewModel
            {
                Id = Guid.NewGuid().ToString().Replace("-", ""),
                Name = "New Chat",
                Time = DateTime.Now
            };
            ChatList.Insert(0,newChat);
            SelectedChat = newChat;

            Prompt = new ChatMessageViewModel
            {
                Id = Guid.NewGuid().ToString().Replace("-", ""),
                Prompt = string.Empty,
                Send = this.Send
            };
        }

        private async Task NewAction()
        {
            NewChat();
            await Task.Yield();
        }

        private async Task Send(ChatMessageViewModel sendMessage)
        {
            if(string.IsNullOrEmpty(sendMessage.Prompt))
            {
                return;
            }

            if(sendMessage.ParentId == null)
            {
                var last = ChatMessageList.LastOrDefault();
                if(last != null)
                {
                    sendMessage.ParentId = last.Id;
                }
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
                promptMessage.Id = Guid.NewGuid().ToString().Replace("-", "");
                promptMessage.ParentId = sendMessage.ParentId;
                promptMessage.Send = this.Send;
                ChatMessageList.Add(promptMessage);
            }

            var prompt = sendMessage.Prompt;
            promptMessage.Message = prompt;
            promptMessage.Prompt = "";
            promptMessage.IsSent = true;
            promptMessage.IsUser = true;
            promptMessage.Time = DateTime.Now;
            sendMessage.Prompt = "";
            CurrentChatMessage = promptMessage;
            var chatServiceSettings = new ChatServiceSettings
            {
                Model = "gpt-3.5-turbo",
                Messages = chatPrompts,
                Suffix = null,
                Temperature = 0.7m,
                MaxTokens = 512,
                TopP = 1.0m,
                Stop = null,
                ApiKey = ""
            };
            IChatStreamService chatService = _serviceProvider.GetService<IChatStreamService>();
            bool first = true;
            ChatMessageViewModel? lastMessage = null;
            resultMessage = new ChatMessageViewModel
            {
                Id = Guid.NewGuid().ToString().Replace("-", ""),
                IsSent = false,
            };
            resultMessage.Send = this.Send;
            await chatService.GetResponseDataAsync(chatServiceSettings,
                (responseStr) => 
                {
                    if (first)
                    {
                        ChatMessageList.Add(resultMessage);
                    }
                    if ("[DONE]".Equals(responseStr))
                    {
                        resultMessage.IsSent = true;
                        resultMessage.IsUser = false;
                        resultMessage.Time = DateTime.Now;
                        resultMessage.Prompt = prompt;
                        resultMessage.ParentId = promptMessage.Id;
                    }
                    else
                    {
                        resultMessage.Message += responseStr;
                    }
                    if (ChatMessageList.LastOrDefault() == resultMessage)
                    {
                        resultMessage.IsSent = false;
                    }
                    lastMessage = resultMessage;
                    first = false;
                });
            //set resultMessage to current
            CurrentChatMessage = lastMessage;
            //set resultMessage to promptMessage's result
            promptMessage.Result = lastMessage;
            await Save(SelectedChat, ChatMessageList);
            IsEnabled = true;
        }

        private async Task Save(ChatViewModel chatViewModel, ObservableCollection<ChatMessageViewModel> chatMessageViewModels)
        {
            using(var db = new LiteDatabase(@".\store.db"))
            {
                var chats = db.GetCollection<ChatDo>("chat");

                ChatDo exists = chats.FindOne(p => p.Id.Equals(chatViewModel.Id));
                if(exists != null) //存在chat则更新
                {
                    var messages = new List<MessageDo>();
                    foreach (var messageViewModel in chatMessageViewModels)
                    {
                        MessageDo message = new MessageDo();
                        message.id = messageViewModel.Id;
                        message.parentId = messageViewModel.ParentId;
                        message.chatId = chatViewModel.Id;
                        message.isUser = messageViewModel.IsUser;
                        message.role = messageViewModel.IsUser ? "user" : "assistant";
                        message.message = messageViewModel.Message;
                        message.time = messageViewModel.Time;
                        messages.Add(message);
                    }
                    exists.messages = messages;
                    chats.Update(exists);
                }
                else //不存在则新增
                {
                    var newChat = new ChatDo
                    {
                        Id = chatViewModel.Id,
                        name = chatViewModel.Name,
                        time = DateTime.Now
                    };
                    var messages = new List<MessageDo>();
                    foreach (var messageViewModel in chatMessageViewModels)
                    {
                        MessageDo message = new MessageDo();
                        message.id = messageViewModel.Id;
                        message.parentId = messageViewModel.ParentId;
                        message.chatId = chatViewModel.Id;
                        message.isUser = messageViewModel.IsUser;
                        message.role = messageViewModel.IsUser ? "user" : "assistant";
                        message.message = messageViewModel.Message;
                        message.time = messageViewModel.Time;
                        messages.Add(message);
                    }
                    newChat.messages = messages;
                    chats.Insert(newChat);
                }
            }
        }

        private void SaveConfig()
        {

        }

        private void LoadHistory()
        {
            using (var db = new LiteDatabase(@".\store.db"))
            {
                var chats = db.GetCollection<ChatDo>("chat");
                IEnumerable<ChatDo> chatAll = chats.FindAll();
                ChatList = new ObservableCollection<ChatViewModel>(
                        chatAll.Select(p =>
                        {
                            ChatViewModel chatViewModel = new ChatViewModel { Id = p.Id, Name = p.name, Time = p.time };
                            var allMessages = p.messages.Select(p =>
                                   new ChatMessageViewModel
                                   {
                                       Id = p.id,
                                       IsUser = p.isUser,
                                       Message = p.message,
                                       Time = p.time,
                                       ParentId = p.parentId
                                   }
                               );
                            foreach (var message in allMessages)
                            {
                                if (message.IsUser)
                                {
                                    message.Result = allMessages.FirstOrDefault(p => p.ParentId == message.Id);
                                }
                            }
                            chatViewModel.ChatMessageList = allMessages.ToList();
                            return chatViewModel;
                        }
                    ).OrderByDescending(p => p.Time).ToList());
                SelectedChat = ChatList?.FirstOrDefault();
            }
        }

        [RelayCommand]
        //打开设置页面
        public void Setting()
        {

        }

        public void Dispose()
        {
            this.PropertyChanged -= MainViewModel_PropertyChanged;
        }

    }
}

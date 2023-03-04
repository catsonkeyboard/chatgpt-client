using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatWpfUI
{
    public class ChatMessageViewModel : ObservableObject
    {
        public bool IsUser { get; set; }

        public string Avatar { get; set; }

        public string Message { get; set; }

        public string Prompt { get; set; }

        public DateTime Time { get; set; }

        public bool IsEditing { get; set; }

        public bool IsSent { get; set; }

        public bool IsError { get; set; }

        public bool IsAwaiting { get; set; }

        public ChatMessageViewModel Result { get; set; }

        public Func<ChatMessageViewModel, Task>? Send { get;set; }

        public IRelayCommand SendCommand { get; set; }

        public ChatMessageViewModel()
        {
            SendCommand = new AsyncRelayCommand(async _ => await SendAction() );
        }

        private async Task SendAction()
        {
            if(Send is { })
            {
                IsEditing = false;
                await Send(this);
            }
        }
    }
}

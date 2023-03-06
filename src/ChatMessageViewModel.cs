using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatWpfUI
{
    public partial class ChatMessageViewModel : ObservableObject
    {
        public string Id { get; set; }

        public bool IsUser { get; set; }

        public string Avatar { get; set; }

        [ObservableProperty]
        private string _message;

        [ObservableProperty]
        private string _prompt;

        public DateTime Time { get; set; }

        [ObservableProperty]
        private bool _isEditing;

        [ObservableProperty]
        private bool _isSent;

        [ObservableProperty]
        private bool _isError;

        [ObservableProperty]
        private bool _isAwaiting;

        public ChatMessageViewModel Result { get; set; }

        public string ParentId { get; set; }

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

using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;

namespace ChatWpfUI
{
    public class ChatViewModel : ObservableObject
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public DateTime Time { get; set; }

        public string Avatar { get; set; }

        public List<ChatMessageViewModel> ChatMessageList { get; set; } 
    }
}

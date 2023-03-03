using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatWpfUI
{
    public class ChatViewModel : ObservableObject
    {
        public bool IsUser { get; set; }

        public string Avatar { get; set; }

        public string Message { get; set; }

        public DateTime Time { get; set; }

    }
}

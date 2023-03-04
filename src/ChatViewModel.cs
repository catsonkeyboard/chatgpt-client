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
        public string Id { get; set; }
        public string Name { get; set; }

        public string Avatar { get; set; }
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatWpfUI
{
    public class ChatHistoryViewModel : ObservableObject
    {
        public string Name { get; set; }

        public string Avatar { get; set; }
    }
}

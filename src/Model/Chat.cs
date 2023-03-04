using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatWpfUI.Model
{
    public record ChatDo
    {
        public string Id { get; set; }

        public string name { get; set; }

        public List<MessageDo> messages { get; set; }
        
    }

    public record MessageDo
    {
        public string id { get; set; }

        public string parentId { get; set; }

        public string chatId { get; set; }

        public string message { get; set; }

        public string role { get; set; }

        public bool isUser { get; set; }

    }
}

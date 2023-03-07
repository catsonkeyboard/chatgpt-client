using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatWpfUI.Model
{
    public record Config
    {
        public string Id { get; set; }

        public string ApiKey { get; set; }

        public string Proxy { get; set; }
    }
}

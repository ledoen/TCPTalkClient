using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPTalkClient
{
    public class MessageModel
    {
        public string SendTime { get; set; }
        public string MessageContent { get; set; }

        public MessageModel()
        {
            SendTime = DateTime.Now.ToString("dd-hh-mm-ss");
        }
    }
}

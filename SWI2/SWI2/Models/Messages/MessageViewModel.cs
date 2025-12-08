using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SWI2.Models.Messages
{
    public class MessageViewModel
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime? Posted { get; set; }
        public DateTime? Readed { get; set; }
        [Required]
        public string MessageReceiverId { get; set; }
        public string MessageReceiverName { get; set; }
        public string MessageSenderName { get; set; }
    }
}

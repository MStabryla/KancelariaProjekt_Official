using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWI2DB.Models.Messages
{
    public class Message : BaseModel
    {
        [StringLength(200, MinimumLength = 0, ErrorMessage = "Title should be minimum 1 characters and a maximum of 200 characters")]
        public string Title { get; set; }
        [StringLength(100000, MinimumLength = 0, ErrorMessage = "Content should be minimum 1 characters and a maximum of 100000 characters")]
        public string Content { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime Posted { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime Readed { get; set; }
        [ForeignKey("MessageReceiver")]
        public long? MessageReceiverId { get; set; }
        [ForeignKey("MessageSender")]
        public long? MessageSenderId { get; set; }
        public virtual MessageReceiver MessageReceiver { get; set; }
        public virtual MessageSender MessageSender { get; set; }
        public long? OldReceiverId { get; set; }

        // Proponujê ustawiæ transkacjê na SQL, która bêdzie usuwaæ wiadomoœci le¿¹ce d³u¿ej w koszu ni¿ 30 dni.
        public bool Trashbox { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime SendedToTrashbox { get; set; }

    }
}

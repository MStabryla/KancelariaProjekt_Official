using System;
using System.ComponentModel.DataAnnotations;

namespace SWI2DB.Models.Administrator
{
    public class Log : BaseModel
    {
        public string Application { get; set; }
        public DateTime Logged { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public string Logger { get; set; }
        public string Callsite { get; set; }
        public string Exception { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
        public string Companys { get; set; }
        public int EventId { get; set; }
        public string EventName { get; set; }
    }
}
/*[Id] [int] IDENTITY(1,1) NOT NULL,

[Application] [nvarchar](50) NOT NULL,

[Logged] [datetime] NOT NULL,

[Level] [nvarchar](50) NOT NULL,

[Message] [nvarchar](max)NOT NULL,
	[Logger] [nvarchar](250) NULL,
	[Callsite] [nvarchar](max)NULL,
	[Exception] [nvarchar](max)NULL,*/
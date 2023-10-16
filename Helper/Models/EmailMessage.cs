using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helper.Models
{
    public class EmailMessage
    {
        public string Subject { get; set; }
        public string EmailTo { get; set; }
        public string Content { get; set; }
    }
}

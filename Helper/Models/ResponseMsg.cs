using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helper.Models
{
    public class ResponseMsg
    {
        public Boolean status { get; set; }
        public dynamic data { get; set; }
        public string message { get; set; } 
    }
}

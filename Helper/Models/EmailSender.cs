using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helper.Models
{
    [Table("EmailSender", Schema = "dbo")]
    public class EmailSender
    {
        [Key]
        public string usr { get; set; }
        public string pwd { get; set; }
        public int EmailSended { get; set; }
    }
}

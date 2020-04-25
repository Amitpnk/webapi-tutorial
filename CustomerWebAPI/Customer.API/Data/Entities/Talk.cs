using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Customer.API.Data.Entities
{
    public class Talk
    {
        public int TalkId { get; set; }
        public Camp Camp { get; set; }
        public string Title { get; set; }
        public string Abstract { get; set; }
        public int Level { get; set; }
        public Speaker Speaker { get; set; }
    }
}
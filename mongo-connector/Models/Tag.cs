
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoConnector.Models
{
    public class Tag
    {
        public string? Id { get; set; }
        public string TagName { get; set; }
        public string Field { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoConnector.Models
{
    public class Recipe
    {
        public string? Id { get; set; }
        public string Title { get; set; }
        public string Video { get; set; }
        public List<string> Image { get; set; }
        public string Instructions { get; set; }
        public Difficulty Difficulty { get; set; } = Difficulty.Easy;
        public string PortionType { get; set; }
        public TimeSpan PrepTime { get; set; }
        public TimeSpan CookTime { get; set; }
        public string CreatorId { get; set; }
        public Status Status { get; set; } = Status.Draft;
    }
    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }
    public enum Status
    {
        Draft,
        Pending,
        Approved,
        Rejected
    }
}

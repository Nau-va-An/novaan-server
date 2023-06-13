using System;
namespace MongoConnector.Models
{
    public class Instruction
    {
        public int Step { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
    }
}


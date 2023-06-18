using System;
namespace MongoConnector.Models
{
    public class Instruction
    {
        public int Step { get; set; }

        public string Description { get; set; } = string.Empty;

        public string? Image { get; set; }
    }
}


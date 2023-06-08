using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoConnector.Models
{
    public class RecipeIngredient
    {
        public string RecipeId { get; set; }
        public string IngredientId { get; set;}
        public int Amount { get; set; }
        public Unit Unit { get; set; }

    }
    public enum Unit
    {
        g,
        kg,
        l, 
        ml
    }
}

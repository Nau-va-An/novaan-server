using System.ComponentModel.DataAnnotations;
using NovaanServer.src.Common.DTOs;

namespace NovaanServer.src.Search.DTOs
{
    public class AdvancedSearchRecipesReq
    {
        [Required]
        public List<string> Ingredients { get; set; } = new List<string>();

        public Pagination? pagination { get; set; } = new Pagination();
    }
}
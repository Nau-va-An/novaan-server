using System.ComponentModel.DataAnnotations;
using NovaanServer.src.Common.DTOs;

namespace NovaanServer.src.Search.DTOs
{
    public class AdvancedSearchReq
    {
        [Required]
        public List<string> Ingredients { get; set; } = new List<string>();

        public Pagination? Pagination { get; set; } = new Pagination();
    }
}
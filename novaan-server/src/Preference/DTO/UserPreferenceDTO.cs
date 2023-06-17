using System.ComponentModel.DataAnnotations;

namespace NovaanServer.src.Preference.DTOs
{
    public class UserPreferenceDTO
    {
        [Required]
        public List<string> Diets { get; set; } = new List<string>();

        [Required]
        public List<string> Cuisines { get; set; } = new List<string>();

        [Required]
        public List<string> Allergens { get; set; } = new List<string>();
    }
}


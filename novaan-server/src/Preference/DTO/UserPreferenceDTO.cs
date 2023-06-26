using System.ComponentModel.DataAnnotations;

namespace NovaanServer.src.Preference.DTOs
{
    public class UserPreferenceDTO
    {
        [Required]
        public HashSet<string> Diets { get; set; } = new HashSet<string>();

        [Required]
        public HashSet<string> Cuisines { get; set; } = new HashSet<string>();

        [Required]
        public HashSet<string> Allergens { get; set; } = new HashSet<string>();
    }
}


using NovaanServer.src.Preference.DTOs;

namespace NovaanServer.src.Preference
{
    public interface IPreferenceService
    {
        PreferenceDTO GetAllPreferences();
        UserPreferenceDTO GetPreference(string userId);
        Task UpdatePreference(string userId, List<string> dietId, List<string> cuisineId, List<string> mealTypeId);
    }
}
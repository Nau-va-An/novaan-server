using NovaanServer.src.Preference.DTOs;

namespace NovaanServer.src.Preference
{
    public interface IPreferenceService
    {
        PreferenceDTO GetAllPreferences();
        UserPreferenceDTO GetPreference(string userId);
        Task UpdatePreference(UserPreferenceDTO userPreferenceDTO);
    }
}
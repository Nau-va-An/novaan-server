using NovaanServer.src.Preference.DTOs;

namespace NovaanServer.src.Preference
{
    public interface IPreferenceService
    {
        AllPreferencesDTO GetAllPreferences();
        Task<UserPreferenceDTO> GetUserPreferences(string userId);
        Task UpdateUserPreferences(string userId, UserPreferenceDTO userPreferenceDTO);
    }
}
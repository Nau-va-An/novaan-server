using NovaanServer.src.Preference.DTOs;

namespace NovaanServer.src.Preference
{
    public interface IPreferenceService
    {
        AllPreferencesDTO GetAllPreferences();
        Task<UserPreferenceDTO> GetUserPreferences(string? currentUserId);
        Task UpdateUserPreferences(string? currentUserId, UserPreferenceDTO userPreferenceDTO);
    }
}
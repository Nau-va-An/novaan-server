using NovaanServer.src.Preference.DTOs;

namespace NovaanServer.src.Preference
{
    public interface IPreferenceService
    {
        List<PreferenceDTO> GetAllPreference();
    }
}
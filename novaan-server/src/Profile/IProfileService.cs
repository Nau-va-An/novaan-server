using System;
using MongoConnector.Models;
using NovaanServer.src.Profile.DTOs;

namespace NovaanServer.src.Profile
{
    public interface IProfileService
    {
        Task<ProfileRESDTO> GetMyProfile(string currentUser);
        ProfileRESDTO GetProfile(string userID, string userID1);
    }
}


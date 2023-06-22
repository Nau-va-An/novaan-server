using System;
using MongoConnector.Models;
using NovaanServer.src.Profile.DTOs;

namespace NovaanServer.src.Profile
{
    public interface IProfileService
    {
       public Task<ProfileRESDTO> GetProfile(string currentUserId, string userID);
    }
}


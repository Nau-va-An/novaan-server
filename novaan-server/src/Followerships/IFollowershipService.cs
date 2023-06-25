using MongoConnector.Models;
using NovaanServer.src.Common.DTOs;
using NovaanServer.src.Followerships.DTOs;

namespace NovaanServer.src.Followerships
{
    public interface IFollowershipService
    {
        public Task FollowUser(string? currentUserID, string followingUserId);
        public Task<List<FollowershipDTO>> GetFollowers(string? currentUserID, string userId, Pagination pagination);
        public Task<List<FollowershipDTO>> GetFollowing(string? currentUserID, string userId, Pagination pagination);
        public Task UnfollowUser(string? currentUserID, string followingUserId);
    }
}
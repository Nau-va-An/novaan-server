using MongoConnector.Models;
using NovaanServer.src.Common.DTOs;
using NovaanServer.src.Followerships.DTOs;

namespace NovaanServer.src.Followerships
{
    public interface IFollowershipService
    {
        public Task FollowUser(string currentUserID, string followingUserId);
        List<FollowershipDTO> GetFollowers(string userId, Pagination pagination);
        List<FollowershipDTO> GetFollowing(string userId, Pagination pagination);
        public Task UnfollowUser(string currentUserID, string followingUserId);
    }
}
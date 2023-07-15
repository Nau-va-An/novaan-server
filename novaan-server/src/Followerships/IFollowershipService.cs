using MongoConnector.Models;
using NovaanServer.src.Common.DTOs;
using NovaanServer.src.Followerships.DTOs;

namespace NovaanServer.src.Followerships
{
    public interface IFollowershipService
    {
        public Task FollowUser(string currentUserId, string followingUserId);
        public Task<List<FollowershipDTO>> GetFollowers(
            string currentUserId,
            string userId,
            Pagination pagination
        );
        public Task<List<FollowershipDTO>> GetFollowing(
            string currentUserId,
            string userId,
            Pagination pagination
        );
        public Task UnfollowUser(string currentUserId, string followingUserId);
    }
}
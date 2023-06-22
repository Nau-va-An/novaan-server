using MongoConnector.Models;
using NovaanServer.src.Followerships.DTOs;

namespace NovaanServer.src.Followerships
{
    public interface IFollowershipService
    {
        public Task FollowUser(FollowershipDTO followership);
        List<User> GetFollowers(string userId);
        List<User> GetFollowing(string userId);
        public Task UnfollowUser(FollowershipDTO followUserDTO);
    }
}
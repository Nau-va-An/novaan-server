using NovaanServer.src.Followerships.DTOs;

namespace NovaanServer.src.Followerships
{
    public interface IFollowershipService
    {
        public Task FollowUser(FollowershipDTO followership);
        public Task UnfollowUser(FollowershipDTO followUserDTO);
    }
}
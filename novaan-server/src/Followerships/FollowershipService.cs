using System.Net;
using MongoConnector;
using MongoConnector.Models;
using MongoDB.Driver;
using NovaanServer.src.Common.DTOs;
using NovaanServer.src.ExceptionLayer.CustomExceptions;
using NovaanServer.src.Followerships.DTOs;

namespace NovaanServer.src.Followerships
{
    public class FollowershipService : IFollowershipService
    {
        public MongoDBService _mongodbService { get; set; }

        public FollowershipService(MongoDBService mongoDBService)
        {
            _mongodbService = mongoDBService;
        }

        public async Task FollowUser(string? currentUserId, string followingUserId)
        {
            if (currentUserId == null)
            {
                throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.NotFound);
            }

            if (currentUserId == followingUserId)
            {
                throw new NovaanException(ErrorCodes.USER_FOLLOWING_ITSELF, HttpStatusCode.BadRequest);
            }

            // Check if user is already following the followed user
            var followership = (await _mongodbService.Followerships
                .FindAsync(f => f.FollowerId == currentUserId && f.FollowingId == followingUserId))
                .FirstOrDefault();
            if (followership != null)
            {
                throw new NovaanException(ErrorCodes.USER_ALREADY_FOLLOWING, HttpStatusCode.BadRequest);
            }

            // Mapping to Model
            var followershipModel = new Followership
            {
                FollowerId = currentUserId,
                FollowingId = followingUserId
            };

            try
            {
                await _mongodbService.Followerships.InsertOneAsync(followershipModel);

                // Increase following count of user that has id is currentUserId
                var update = Builders<User>.Update.Inc(u => u.FollowingCount, 1);
                await _mongodbService.Users.UpdateOneAsync(u => u.Id == currentUserId, update);

                // Increase follower count of user that has id is followingId
                update = Builders<User>.Update.Inc(u => u.FollowerCount, 1);
                await _mongodbService.Users.UpdateOneAsync(u => u.Id == followingUserId, update);
            }
            catch (System.Exception)
            {
                throw new NovaanException(ErrorCodes.FOLLOWERSHIP_NOT_CREATED);
            }
        }

        public async Task UnfollowUser(string? currentUserId, string followingUserId)
        {
            if (currentUserId == null)
            {
                throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.NotFound);
            }

            if (currentUserId == followingUserId)
            {
                throw new NovaanException(ErrorCodes.USER_FOLLOWING_ITSELF, HttpStatusCode.BadRequest);
            }

            // Check if user is already following the followed user
            Followership followership = (await _mongodbService.Followerships
                .FindAsync(
                    f => f.FollowerId == currentUserId &&
                    f.FollowingId == followingUserId
                ))
                .FirstOrDefault();
            if (followership == null)
            {
                throw new NovaanException(ErrorCodes.USER_NOT_FOLLOWING, HttpStatusCode.BadRequest);
            }

            try
            {
                // Delete followership
                await _mongodbService.Followerships
                    .DeleteOneAsync(
                        f => f.FollowerId == currentUserId &&
                        f.FollowingId == followingUserId
                    );

                // Decrease following count of user that has id is userId
                var update = Builders<User>.Update.Inc(u => u.FollowingCount, -1);
                await _mongodbService.Users.UpdateOneAsync(u => u.Id == currentUserId, update);

                // Decrease follower count of user that has id is followedId
                update = Builders<User>.Update.Inc(u => u.FollowerCount, -1);
                await _mongodbService.Users.UpdateOneAsync(u => u.Id == followingUserId, update);
            }
            catch (System.Exception)
            {
                throw new NovaanException(ErrorCodes.FOLLOWERSHIP_NOT_DELETED);
            }
        }

        public async Task<List<FollowershipDTO>> GetFollowers(
            string? currentUserId, 
            string targetuserId, 
            Pagination pagination
        )
        {
            if (currentUserId == null)
            {
                throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.NotFound);
            }

            // Get all followers of user that has id is userId
            List<Followership> followers = (await _mongodbService.Followerships
                .FindAsync(f => f.FollowingId == targetuserId))
                .ToList();
            List<string> followerIds = followers
                .Select(f => f.FollowerId)
                .ToList();

            var filter = Builders<User>.Filter.In(u => u.Id, followerIds);
            List<User> followerUsers = (await _mongodbService.Users
                .FindAsync(filter))
                .ToList();

            // Get all user from pagination.Start to pagination.Start + pagination.Limit
            var start = pagination.Start;
            var limit = pagination.Limit;

            var followerUsersDTO = followerUsers
                .Skip(start)
                .Take(limit)
                .Select(u => new FollowershipDTO
                {
                    UserId = u.Id,
                    UserName = u.DisplayName,
                    Avatar = u.ProfilePicture,
                    // IsFollowed = followers.Any(f => f.FollowerId == currentUser.Id && f.FollowingId == u.Id)
                }).ToList();

            return followerUsersDTO;
        }

        public async Task<List<FollowershipDTO>> GetFollowing(
            string? currentUserId,
            string targetUserId, 
            Pagination pagination
        )
        {
            if (currentUserId == null)
            {
                throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.NotFound);
            }

            List<Followership> following = (await _mongodbService.Followerships
                .FindAsync(f => f.FollowerId == targetUserId))
                .ToList();
            List<string> followingIds = following.Select(f => f.FollowingId).ToList();

            var filter = Builders<User>.Filter.In(u => u.Id, followingIds);
            List<User> followingUsers = (await _mongodbService.Users
                .FindAsync(filter))
                .ToList();

            // Get all user from pagination.Start to pagination.Start + pagination.Limit
            var start = pagination.Start;
            var limit = pagination.Limit;

            var followingUsersDTO = followingUsers
                .Skip(start)
                .Take(limit)
                .Select(u => new FollowershipDTO
                {
                    UserId = u.Id,
                    UserName = u.DisplayName,
                    Avatar = u.ProfilePicture,
                    // IsFollowed = following.Any(f => f.FollowerId == currentUser.Id && f.FollowingId == u.Id)
                }).ToList();
            return followingUsersDTO;
        }
    }
}
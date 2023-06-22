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

        public async Task FollowUser(string currentUserID, string followingUserId)
        {
            var currentUser = (await _mongodbService.Users.FindAsync(u => u.AccountID == currentUserID)).FirstOrDefault();
            var followedUser = (await _mongodbService.Users.FindAsync(u => u.Id == followingUserId)).FirstOrDefault();

            if (currentUser == null || followedUser == null)
            {
                throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.NotFound);
            }

            if (currentUser.Id == followingUserId)
            {
                throw new NovaanException(ErrorCodes.USER_FOLLOWING_ITSELF, HttpStatusCode.BadRequest);
            }

            // Check if user is already following the followed user
            var isFollowing = (await _mongodbService.Followerships.FindAsync(f => f.FollowerId == currentUser.Id && f.FollowingId == followingUserId)).FirstOrDefault();
            if (isFollowing != null)
            {
                throw new NovaanException(ErrorCodes.USER_ALREADY_FOLLOWING, HttpStatusCode.BadRequest);
            }

            // Mapping to Model
            var followership = new Followership
            {
                FollowerId = currentUser.Id,
                FollowingId = followingUserId
            };

            try
            {
                await _mongodbService.Followerships.InsertOneAsync(followership);

                // Increase following count of user that has id is currentUserID
                var update = Builders<User>.Update.Inc(u => u.FollowingCount, 1);
                await _mongodbService.Users.UpdateOneAsync(u => u.AccountID == currentUserID, update);

                // Increase follower count of user that has id is followingId
                update = Builders<User>.Update.Inc(u => u.FollowerCount, 1);
                await _mongodbService.Users.UpdateOneAsync(u => u.Id == followingUserId, update);
            }
            catch (System.Exception)
            {
                throw new NovaanException(ErrorCodes.FOLLOWERSHIP_NOT_CREATED, HttpStatusCode.InternalServerError);
            }
        }

        public async Task UnfollowUser(string currentUserID, string followingUserId)
        {
            User user = (await _mongodbService.Users.FindAsync(u => u.AccountID == currentUserID)).FirstOrDefault();
            User followedUser = (await _mongodbService.Users.FindAsync(u => u.Id == followingUserId)).FirstOrDefault();
            if (user == null || followedUser == null)
            {
                throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.NotFound);
            }

            if (user.Id == followingUserId)
            {
                throw new NovaanException(ErrorCodes.USER_FOLLOWING_ITSELF, HttpStatusCode.BadRequest);
            }

            // Check if user is already following the followed user
            Followership isFollowing = (await _mongodbService.Followerships.FindAsync(f => f.FollowerId == user.Id && f.FollowingId == followingUserId)).FirstOrDefault();
            if (isFollowing == null)
            {
                throw new NovaanException(ErrorCodes.USER_NOT_FOLLOWING, HttpStatusCode.BadRequest);
            }

            try
            {
                // Delete followership
                await _mongodbService.Followerships.DeleteOneAsync(f => f.FollowerId == user.Id && f.FollowingId == followingUserId);

                // Decrease following count of user that has id is userId
                var update = Builders<User>.Update.Inc(u => u.FollowingCount, -1);
                await _mongodbService.Users.UpdateOneAsync(u => u.AccountID == currentUserID, update);

                // Decrease follower count of user that has id is followedId
                update = Builders<User>.Update.Inc(u => u.FollowerCount, -1);
                await _mongodbService.Users.UpdateOneAsync(u => u.Id == followingUserId, update);
            }
            catch (System.Exception)
            {
                throw new NovaanException(ErrorCodes.FOLLOWERSHIP_NOT_DELETED, HttpStatusCode.InternalServerError);
            }
        }

        public List<FollowershipDTO> GetFollowers(string userId, Pagination pagination)
        {
            //Get all followers of user that has id is userId
            List<Followership> followers = _mongodbService.Followerships.Find(f => f.FollowingId == userId).ToList();
            List<string> followerIds = followers.Select(f => f.FollowerId).ToList();
            List<User> followerUsers = _mongodbService.Users.Find(u => followerIds.Contains(u.Id)).ToList();

            // Get all user from pagination.Start to pagination.Start + pagination.Limit
            var start = pagination.Start;
            var limit = pagination.Limit;
            var followerUsersDTO = followerUsers.Skip(start).Take(limit).Select(u => new FollowershipDTO
            {
                UserId = u.Id,
                UserName = u.DisplayName,
                Avatar = u.ProfilePicture,
                IsFollowed = followers.Any(f => f.FollowerId == userId && f.FollowingId == u.Id)
            }).ToList();

            return followerUsersDTO;
        }

        public List<FollowershipDTO> GetFollowing(string userId, Pagination pagination)
        {
            List<Followership> following = _mongodbService.Followerships.Find(f => f.FollowerId == userId).ToList();
            List<string> followingIds = following.Select(f => f.FollowingId).ToList();
            List<User> followingUsers = _mongodbService.Users.Find(u => followingIds.Contains(u.Id)).ToList();

            // Get all user from pagination.Start to pagination.Start + pagination.Limit
            var start = pagination.Start;
            var limit = pagination.Limit;
            var followingUsersDTO = followingUsers.Skip(start).Take(limit).Select(u => new FollowershipDTO
            {
                UserId = u.Id,
                UserName = u.DisplayName,
                Avatar = u.ProfilePicture,
                IsFollowed = following.Any(f => f.FollowerId == userId && f.FollowingId == u.Id)
            }).ToList();
            return followingUsersDTO;
        }
    }
}
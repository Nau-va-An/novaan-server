using System.Net;
using MongoConnector;
using MongoConnector.Models;
using MongoDB.Driver;
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
            var currentUser = (await _mongodbService.Users.FindAsync(u => u.Id == currentUserID)).FirstOrDefault();
            var followedUser = (await _mongodbService.Users.FindAsync(u => u.Id == followingUserId)).FirstOrDefault();
            if (currentUser == null || followedUser == null)
            {
                throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.NotFound);
            }

            // Check if user is already following the followed user
            var isFollowing = (await _mongodbService.Followerships.FindAsync(f => f.FollowerId == currentUserID && f.FollowingId == followingUserId)).FirstOrDefault();
            if (isFollowing != null)
            {
                throw new NovaanException(ErrorCodes.USER_ALREADY_FOLLOWING, HttpStatusCode.BadRequest);
            }

            // Mapping to Model
            var followership = new Followership
            {
                FollowerId = currentUserID,
                FollowingId = followingUserId
            };

            try
            {
                await _mongodbService.Followerships.InsertOneAsync(followership);

                // Increase following count of user that has id is currentUserID
                var update = Builders<User>.Update.Inc(u => u.FollowingCount, 1);
                await _mongodbService.Users.UpdateOneAsync(u => u.Id == currentUserID, update);

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
            var user = (await _mongodbService.Users.FindAsync(u => u.Id == currentUserID)).FirstOrDefault();
            var followedUser = (await _mongodbService.Users.FindAsync(u => u.Id == followingUserId)).FirstOrDefault();
            if (user == null || followedUser == null)
            {
                throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.NotFound);
            }

            // Check if user is already following the followed user
            var isFollowing = (await _mongodbService.Followerships.FindAsync(f => f.FollowerId == currentUserID && f.FollowingId == followingUserId)).FirstOrDefault();
            if (isFollowing == null)
            {
                throw new NovaanException(ErrorCodes.USER_NOT_FOLLOWING, HttpStatusCode.BadRequest);
            }

            try
            {
                // Delete followership
                await _mongodbService.Followerships.DeleteOneAsync(f => f.FollowerId == currentUserID && f.FollowingId == followingUserId);

                // Decrease following count of user that has id is userId
                var update = Builders<User>.Update.Inc(u => u.FollowingCount, -1);
                await _mongodbService.Users.UpdateOneAsync(u => u.Id == currentUserID, update);

                // Decrease follower count of user that has id is followedId
                update = Builders<User>.Update.Inc(u => u.FollowerCount, -1);
                await _mongodbService.Users.UpdateOneAsync(u => u.Id == followingUserId, update);
            }
            catch (System.Exception)
            {
                throw new NovaanException(ErrorCodes.FOLLOWERSHIP_NOT_DELETED, HttpStatusCode.InternalServerError);
            }
        }

        public List<User> GetFollowers(string userId)
        {
            var followers = _mongodbService.Followerships.Find(f => f.FollowingId == userId).ToList();
            var followerIds = followers.Select(f => f.FollowerId).ToList();
            var followerUsers = _mongodbService.Users.Find(u => followerIds.Contains(u.Id)).ToList();
            return followerUsers;
        }

        public List<User> GetFollowing(string userId)
        {
            var following = _mongodbService.Followerships.Find(f => f.FollowerId == userId).ToList();
            var followingIds = following.Select(f => f.FollowingId).ToList();
            var followingUsers = _mongodbService.Users.Find(u => followingIds.Contains(u.Id)).ToList();
            return followingUsers;
        }
    }
}
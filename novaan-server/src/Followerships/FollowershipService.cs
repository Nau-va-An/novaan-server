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
        public async Task FollowUser(FollowershipDTO followUserDTO)
        {
            var user = (await _mongodbService.Users.FindAsync(u => u.Id == followUserDTO.UserID)).FirstOrDefault();
            var followedUser = (await _mongodbService.Users.FindAsync(u => u.Id == followUserDTO.FollowedUserId)).FirstOrDefault();
            if (user == null || followedUser == null)
            {
                throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.NotFound);
            }

            // Check if user is already following the followed user
            var isFollowing = (await _mongodbService.Followerships.FindAsync(f => f.FollowerId == followUserDTO.UserID && f.FollowingId == followUserDTO.FollowedUserId)).FirstOrDefault();
            if (isFollowing != null)
            {
                throw new NovaanException(ErrorCodes.USER_ALREADY_FOLLOWING, HttpStatusCode.BadRequest);
            }

            // Mapping from DTO to Model
            var followership = new Followership
            {
                FollowerId = followUserDTO.UserID,
                FollowingId = followUserDTO.FollowedUserId
            };

            try
            {
                await _mongodbService.Followerships.InsertOneAsync(followership);

                // Increase following count of user that has id is userId
                var update = Builders<User>.Update.Inc(u => u.FollowingCount, 1);
                await _mongodbService.Users.UpdateOneAsync(u => u.Id == followUserDTO.UserID, update);

                // Increase follower count of user that has id is followedId
                update = Builders<User>.Update.Inc(u => u.FollowerCount, 1);
                await _mongodbService.Users.UpdateOneAsync(u => u.Id == followUserDTO.FollowedUserId, update);
            }
            catch (System.Exception)
            {
                throw new NovaanException(ErrorCodes.FOLLOWERSHIP_NOT_CREATED, HttpStatusCode.InternalServerError);
            }
        }

        public async Task UnfollowUser(FollowershipDTO followUserDTO)
        {
            var user = (await _mongodbService.Users.FindAsync(u => u.Id == followUserDTO.UserID)).FirstOrDefault();
            var followedUser = (await _mongodbService.Users.FindAsync(u => u.Id == followUserDTO.FollowedUserId)).FirstOrDefault();
            if (user == null || followedUser == null)
            {
                throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.NotFound);
            }

            // Check if user is already following the followed user
            var isFollowing = (await _mongodbService.Followerships.FindAsync(f => f.FollowerId == followUserDTO.UserID && f.FollowingId == followUserDTO.FollowedUserId)).FirstOrDefault();
            if (isFollowing == null)
            {
                throw new NovaanException(ErrorCodes.USER_NOT_FOLLOWING, HttpStatusCode.BadRequest);
            }

            try
            {
                // Delete followership
                await _mongodbService.Followerships.DeleteOneAsync(f => f.FollowerId == followUserDTO.UserID && f.FollowingId == followUserDTO.FollowedUserId);

                // Decrease following count of user that has id is userId
                var update = Builders<User>.Update.Inc(u => u.FollowingCount, -1);
                await _mongodbService.Users.UpdateOneAsync(u => u.Id == followUserDTO.UserID, update);

                // Decrease follower count of user that has id is followedId
                update = Builders<User>.Update.Inc(u => u.FollowerCount, -1);
                await _mongodbService.Users.UpdateOneAsync(u => u.Id == followUserDTO.FollowedUserId, update);
            }
            catch (System.Exception)
            {
                throw new NovaanException(ErrorCodes.FOLLOWERSHIP_NOT_DELETED, HttpStatusCode.InternalServerError);
            }
        }
    }
}
using System;
using System.Net;
using MongoConnector;
using MongoConnector.Enums;
using MongoConnector.Models;
using MongoDB.Driver;
using NovaanServer.src.Common.DTOs;
using NovaanServer.src.ExceptionLayer.CustomExceptions;
using NovaanServer.src.Profile.DTOs;

namespace NovaanServer.src.Profile
{
    public class ProfileService : IProfileService
    {
        private readonly MongoDBService _mongoDBService;

        public ProfileService(MongoDBService mongoDBService)
        {
            _mongoDBService = mongoDBService;
        }

        public async Task<GetProfileResDTO> GetProfile(string? currentUserId, string targetUserId)
        {
            if (currentUserId == null)
            {
                throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.NotFound);
            }


            /* Querry to test in Mongosh
                db.users.aggregate([
                    {
                        $match: {
                            "_id": ObjectId("64940b479b7adc28001f72a2")
                        }
                    },
                    {
                        $lookup: {
                            from: "followerships",
                            localField: "_id",
                            foreignField: "FollowingId",
                            as: "joinedData"
                        }
                    },
                    {
                        $project: {
                            joinedData: 1,
                            _id: 0
                        }
                    }
                ])
                */
            var profile = await _mongoDBService.Users
               .Aggregate()
               .Match(u => u.Id == targetUserId)
               .Lookup(
                   foreignCollection: _mongoDBService.Followerships,
                   localField: u => u.Id,
                   foreignField: f => f.FollowingId,
                   @as: (UserDTO u) => u.Followerships
               )
               .Project(u => new GetProfileResDTO
               {
                   Followerships = u.Followerships,
                   Username = u.DisplayName,
                   UserId = u.Id,
                   Avatar = u.Avatar,
                   FollowersCount = u.FollowerCount,
                   FollowingCount = u.FollowingCount,
                   IsFollowing = u.Followerships.Any()
               })
               .FirstOrDefaultAsync()
               ?? throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.NotFound);
            return profile;
        }

        public async Task<List<Recipe>> GetRecipes(string? currentUserId, string targetUserId, Pagination pagination)
        {
            if (currentUserId == null)
            {
                throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.NotFound);
            }

            var recipes = new List<Recipe>();
            if (currentUserId == targetUserId)
            {
                // get all recipes of current user
                recipes = (await _mongoDBService.Recipes
                    .FindAsync(r => r.CreatorId == currentUserId))
                    .ToList();
            }
            else
            {
                // get public recipes of targetUser
                recipes = (await _mongoDBService.Recipes
                    .FindAsync(r => r.CreatorId == targetUserId && r.Status == Status.Approved))
                    .ToList();
            }

            return recipes
                .Skip(pagination.Start)
                .Take(pagination.Limit)
                .ToList();
        }

        public async Task<List<SavedPost>> GetSavedPosts(string? currentUserId, string targetUserId, Pagination pagination)
        {
            if (currentUserId == null)
            {
                throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.NotFound);
            }

            var targetUser = (await _mongoDBService.Users
                .FindAsync(u => u.Id == targetUserId))
                .FirstOrDefault() ??
                throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.NotFound);

            // Only current user can see his/her saved posts
            if (currentUserId != targetUser.Id)
            {
                // forbidden content
                throw new NovaanException(ErrorCodes.FORBIDDEN_PROFILE_CONTENT, HttpStatusCode.Forbidden);
            }
            var savedPost = targetUser.SavedPosts
                .Skip(pagination.Start)
                .Take(pagination.Limit)
                .ToList();
            return savedPost;
        }

        public async Task<List<CulinaryTip>> GetTips(string? currentUserId, string targetUserId, Pagination pagination)
        {
            if (currentUserId == null)
            {
                throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.NotFound);
            }

            var tips = new List<CulinaryTip>();
            if (currentUserId == targetUserId)
            {
                // get all tips of the current user
                tips = (await _mongoDBService.CulinaryTips
                    .FindAsync(t => t.CreatorId == currentUserId))
                    .ToList();
            }
            else
            {
                // get public tips of the targetUser
                tips = (await _mongoDBService.CulinaryTips
                    .FindAsync(t => t.CreatorId == targetUserId && t.Status == Status.Approved))
                    .ToList();
            }
            return tips
                .Skip(pagination.Start)
                .Take(pagination.Limit)
                .ToList();
        }
    }
}


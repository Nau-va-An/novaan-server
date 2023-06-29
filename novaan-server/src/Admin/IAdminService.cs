using System;
using MongoConnector.Enums;
using NovaanServer.src.Admin.DTOs;

namespace NovaanServer.src.Admin
{
    public interface IAdminService
    {
        Task<SubmissionsDTO> GetSubmissions(List<Status> status);
        Task UpdateStatus<T>(UpdateStatusDTO statusDTO, string collectionName);
    }
}


using System;
using MongoConnector.Enums;
using NovaanServer.src.Admin.DTOs;

namespace NovaanServer.src.Admin
{
    public interface IAdminService
    {
        SubmissionsDTO GetSubmissions(List<Status> status);
        void UpdateStatus<T>(UpdateStatusDTO statusDTO, string collectionName);
    }
}


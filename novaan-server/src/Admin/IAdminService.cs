using System;
using MongoConnector.Enums;
using NovaanServer.src.Admin.DTOs;

namespace NovaanServer.src.Admin
{
    public interface IAdminService
    {
        SubmissionsDTO GetSubmissions(Status status);
        void UpdateStatus<T>(StatusDTO statusDTO, string collectionName);
    }
}


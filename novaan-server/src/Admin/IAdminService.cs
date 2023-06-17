using System;
using MongoConnector.Enums;
using NovaanServer.src.Admin.DTOs;

namespace NovaanServer.src.Admin
{
    public interface IAdminService
    {
        List<SubmissionsDTO> GetSubmissions(Status status);
        void UpdateStatus<T>(StatusDTO statusDTO);
    }
}


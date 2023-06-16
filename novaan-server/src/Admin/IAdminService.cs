using System;
using NovaanServer.src.Admin.DTOs;

namespace NovaanServer.src.Admin
{
    public interface IAdminService
    {
        List<SubmissionsDTO> GetSubmissions(int status);
        void UpdateStatus<T>(StatusDTO statusDTO);
    }
}


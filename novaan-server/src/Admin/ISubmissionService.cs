﻿using System;
using NovaanServer.src.Admin.DTOs;

namespace NovaanServer.src.Admin
{
    public interface ISubmissionService
    {
        List<SubmissionsDTO> GetSubmissions(int status);
        Task UpdateStatus<T>(string submissionType,string id, int status);
    }
}


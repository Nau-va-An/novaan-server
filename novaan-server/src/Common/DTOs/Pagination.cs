using System;
using NovaanServer.src.Common.Settings;

namespace NovaanServer.src.Common.DTOs
{
    public class Pagination
    {
        public int Start { get; set; } = PaginationSettings.Start;
        public int Limit { get; set; } = PaginationSettings.Limit;
    }
}


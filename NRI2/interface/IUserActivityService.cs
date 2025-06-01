using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NRI.Data;
using NRI.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

namespace NRI
{
    public interface IUserActivityService
    {
        void StartTracking();
        void StopTracking();
        Task<List<UserStatusDto>> GetActiveUsersAsync();
        Task<UserStatisticsDto> GetUserStatisticsAsync();
    }
}

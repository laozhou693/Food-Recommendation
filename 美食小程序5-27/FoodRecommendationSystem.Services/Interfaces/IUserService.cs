
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoodRecommendationSystem.Core.Models;

namespace FoodRecommendationSystem.Services.Interfaces
{
    public interface IUserService
    {
        Task<User> ValidateUserAsync(string username, string email, string password);
        Task<(bool Success, string Message)> CreateUserAsync(User user, string password);
        // 其他用户相关方法...
    }
}

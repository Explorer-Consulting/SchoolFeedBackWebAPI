using Application.DTOs.Users;
using Application.Services.Interfaces;
using FeedBackApp.Core.Repositories;
using Microsoft.Extensions.Logging;
using NUlid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class UserService(IUserRepository repository, ILogger<UserService> logger) : IUserService
    {
        public Task RemoveAllUsersAsync(IEnumerable<Ulid> userIds)
        {
            throw new NotImplementedException();
        }

        public Task RemoveUserAsync(Ulid userId)
        {
            throw new NotImplementedException();
        }

        public Task<UserDTO> RetrieveUserAsync(Ulid userId)
        {
            throw new NotImplementedException();
        }

        public Task<UserDTO> RetrieveUsersAsync(IEnumerable<Ulid> userIds)
        {
            throw new NotImplementedException();
        }

        public Task UpsertUserAsync()
        {
            throw new NotImplementedException();
        }
    }
}

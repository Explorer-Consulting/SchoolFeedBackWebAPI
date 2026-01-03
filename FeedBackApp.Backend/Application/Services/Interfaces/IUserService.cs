using Application.DTOs.Users;
using NUlid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Interfaces
{
    public interface IUserService
    {
        Task UpsertUserAsync(/*DTO type for user creation*/);

        Task RemoveUserAsync(Ulid userId);

        Task<UserDTO> RetrieveUserAsync(Ulid userId);

        Task RemoveAllUsersAsync(IEnumerable<Ulid> userIds);

        Task<UserDTO> RetrieveUsersAsync(IEnumerable<Ulid> userIds);
    }
}

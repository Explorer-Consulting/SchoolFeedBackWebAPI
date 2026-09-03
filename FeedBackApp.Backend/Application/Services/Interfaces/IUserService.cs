using Application.DTOs.Users;
using NUlid;

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

using API.Entites;
using API.Helpers;

namespace API.Interfaces
{
    public interface IUserRepository
    {
        void Update(AppUser user);
        Task<bool> SaveAllAsync();
        Task<IEnumerable<AppUser>> GetUsersAsync();
        Task<AppUser> GetUserByIdAsync(int id);
        Task<AppUser> GetUserByUsername(string username);
        Task<PagedList<MemeberDto>> GetMembersAsync(UserParams userParams);
        Task<MemeberDto> GetMemeberAsync(string username);
    }

}
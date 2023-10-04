using KidsMealApi.DataAccess.Models;

namespace KidsMealApi.Modules.Users.Ports
{
    public interface IRefreshTokenHistoryService
    {
        Task ClearAllAsync(int userId);
        IEnumerable<RefreshTokenHistory> GetTokenFamily(int userId);
        Task RecycleTokenFamilyAsync(int userId, string refreshToken);
    }
}
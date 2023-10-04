using KidsMealApi.DataAccess;
using KidsMealApi.DataAccess.Models;
using KidsMealApi.Modules.Users.Ports;

namespace KidsMealApi.Modules.Users.Adapters
{
    public class RefreshTokenHistoryService : BaseDataAccessService<RefreshTokenHistory>, IRefreshTokenHistoryService
    {
        public RefreshTokenHistoryService(KidsMealDbContext dbContext) : base(dbContext)
        {

        }

        public async Task ClearAllAsync(int userId)
        {
            //Clear any previous refresh token histories
            var existingTokenHistories = GetAll().Where(h => h.UserId == userId);
            _dbContext.RefreshTokenHistories.RemoveRange(existingTokenHistories);

            await _dbContext.SaveChangesAsync();
        }

        public IEnumerable<RefreshTokenHistory> GetTokenFamily(int userId)
        {
            return GetAll().Where(h => h.UserId == userId);
        }

        public async Task RecycleTokenFamilyAsync(int userId, string refreshToken)
        {
            //Maintain a 24-hour period of token histories; remove older token histories
            var dateTimeCutoff = DateTime.UtcNow.AddDays(-1);
            var outdatedTokenHistories = GetAll().Where(h => h.UserId == userId && h.RevokedOn <= dateTimeCutoff);
            _dbContext.RefreshTokenHistories.RemoveRange(outdatedTokenHistories);

            //Add the old refresh token to the token family for re-use detection
            _dbContext.RefreshTokenHistories.Add(new RefreshTokenHistory(userId, refreshToken));
            await _dbContext.SaveChangesAsync();
        }

        protected override IQueryable<RefreshTokenHistory> GetAll()
        {
            return _dbContext.RefreshTokenHistories;
        }
    }
}
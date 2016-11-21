using System.Data.Entity;
using System.Threading.Tasks;

namespace OnionIdentity.Data.Infrastructure
{
    public interface IUnitOfWork
    {
        void SaveChanges();
        Task SaveChangesAsync();
    }

    internal class UnitOfWork : IUnitOfWork
    {
        private readonly IDbFactory _dbFactory;
        private DbContext _dbContext;
        private DbContext Db => _dbContext ?? (_dbContext = _dbFactory.Init());

        public UnitOfWork(IDbFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }
        
        public void SaveChanges()
        {
            Db.SaveChanges();
        }

        public async Task SaveChangesAsync()
        {
            await Db.SaveChangesAsync();
        }
    }
}

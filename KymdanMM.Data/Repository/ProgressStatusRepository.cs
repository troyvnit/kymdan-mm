using KymdanMM.Model.Models;
using KymdanMM.Data.Infrastructure;

namespace KymdanMM.Data.Repository
{
    public class ProgressStatusRepository : RepositoryBase<ProgressStatus>, IProgressStatusRepository
    {
        public ProgressStatusRepository(IDatabaseFactory databaseFactory)
            : base(databaseFactory)
        {
        }
    }

    public interface IProgressStatusRepository : IRepository<ProgressStatus>
    {
    }
}

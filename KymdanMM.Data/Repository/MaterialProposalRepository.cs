using KymdanMM.Model.Models;
using KymdanMM.Data.Infrastructure;

namespace KymdanMM.Data.Repository
{
    public class MaterialProposalRepository : RepositoryBase<MaterialProposal>, IMaterialProposalRepository
    {
        public MaterialProposalRepository(IDatabaseFactory databaseFactory)
            : base(databaseFactory)
        {
        }
    }

    public interface IMaterialProposalRepository : IRepository<MaterialProposal>
    {
    }
}

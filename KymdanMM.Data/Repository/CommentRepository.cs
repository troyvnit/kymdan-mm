using KymdanMM.Model.Models;
using KymdanMM.Data.Infrastructure;

namespace KymdanMM.Data.Repository
{
    public class CommentRepository : RepositoryBase<Comment>, ICommentRepository
    {
        public CommentRepository(IDatabaseFactory databaseFactory)
            : base(databaseFactory)
        {
        }
    }

    public interface ICommentRepository : IRepository<Comment>
    {
    }
}

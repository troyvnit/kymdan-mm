using System.Linq;

namespace KymdanMM.Data.Infrastructure
{
    public class Page
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        public Page()
        {
            PageNumber = 1;
            PageSize = 10;
        }

        public Page(int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
        }

        public int Skip
        {
            get { return (PageNumber - 1) * PageSize; }
        }
    }

    public static class PagingExtensions
    {
        public static IQueryable<T> GetPage<T>(this IQueryable<T> queryable, Page page)
        {
            return page.PageSize == 1 ? queryable : queryable.Skip(page.Skip).Take(page.PageSize);
        }
    }
}

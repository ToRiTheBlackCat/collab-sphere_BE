using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Common
{
    public class PagedList<T> where T : class
    {
        private readonly IEnumerable<T> _originList;

        public IEnumerable<T> List { get; private set; } = new List<T>();

        public int ItemCount { get; private set; }

        public int PageNum { get; private set; }

        public int PageSize { get; private set; } = 10;

        public int PageCount => (int)Math.Ceiling(_originList.Count() * 1.0 / PageSize);

        public PagedList(IEnumerable<T> list, int pageNum = 1, int pageSize = 0)
        {
            _originList = list;
            ItemCount = list.Count();
            PageSize = pageSize > 0 ? pageSize : PageSize;

            SetPage(pageNum);
        }

        public void SetPage(int pageNumber)
        {
            if (pageNumber <= 0)
            {
                pageNumber = 1;
            }

            PageNum = Math.Min(PageCount, pageNumber);
            List = _originList.Skip((PageNum - 1) * PageSize).Take(PageSize);
        }

        public List<T> GetPageItems(int pageNumber = 0)
        {
            SetPage(pageNumber);

            return List.ToList();
        }
    }
}

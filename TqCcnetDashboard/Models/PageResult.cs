using PagedList;
using System.Collections.Generic;

namespace TqCcnetDashboard
{
    public class PageResult<T> : IPagedList
    {
        private IPagedList<T> _result;

        public PageResult(IPagedList<T> result)
        {
            _result = result;
        }

        public IEnumerable<T> List { get { return _result; } }

        public int FirstItemOnPage
        {
            get
            {
                return _result.FirstItemOnPage;
            }
        }

        public bool HasNextPage
        {
            get
            {
                return _result.HasNextPage;
            }
        }

        public bool HasPreviousPage
        {
            get
            {
                return _result.HasPreviousPage;
            }
        }

        public bool IsFirstPage
        {
            get
            {
                return _result.IsFirstPage;
            }
        }

        public bool IsLastPage
        {
            get
            {
                return _result.IsLastPage;
            }
        }

        public int LastItemOnPage
        {
            get
            {
                return _result.LastItemOnPage;
            }
        }

        public int PageCount
        {
            get
            {
                return _result.PageCount;
            }
        }

        public int PageNumber
        {
            get
            {
                return _result.PageNumber;
            }
        }

        public int PageSize
        {
            get
            {
                return _result.PageSize;
            }
        }

        public int TotalItemCount
        {
            get
            {
                return _result.TotalItemCount;
            }
        }
    }
}
using System;
namespace DatingApp.API.Helpers.RequestParams
{
    public class PaginationParams
    {
        private int _maxPageSize;
        private int pageSize = 10;

        public int PageSize
        {
            get
            {
                return pageSize;
            }
            set { pageSize = (value > _maxPageSize) ? _maxPageSize : value; }
        }

        public int PageNumber { get; set; } = 1;

        public PaginationParams(int maxPageSize)
        {
            _maxPageSize = maxPageSize;
        }
    }
}

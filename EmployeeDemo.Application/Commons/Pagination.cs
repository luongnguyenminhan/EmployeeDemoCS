namespace EmployeeDemo.Application.Commons
{
    public class Pagination<T> : List<T>
    {
        //public int TotalItemsCount { get; set; }
        //public int PageSize { get; set; }
        //public int TotalPagesCount
        //{
        //    get
        //    {
        //        var temp = TotalItemsCount / PageSize;
        //        if (TotalItemsCount % PageSize == 0)
        //        {
        //            return temp;
        //        }
        //        return temp + 1;
        //    }
        //}
        //public int PageIndex { get; set; }

        ///// <summary>
        ///// page number start from 0
        ///// </summary>
        //public bool Next => PageIndex + 1 < TotalPagesCount;
        //public bool Previous => PageIndex > 0;
        //public ICollection<T> Items { get; set; }

        public int CurrentPage { get; private set; }
        public int TotalPages { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
        public Pagination(List<T> items, int count, int pageNumber, int pageSize)
        {
            TotalCount = count;
            PageSize = pageSize;
            CurrentPage = pageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            AddRange(items);
        }
    }
}

using StudentManagement.Models;

namespace StudentManagement.ViewModels
{
    /// <summary>
    /// Composite ViewModel for the Students/Index page.
    /// Bundles the current page of data together with all the state needed to
    /// re-render search box, sort links and pagination controls without losing context.
    /// </summary>
    public class StudentIndexViewModel
    {
        public IEnumerable<Student> Students { get; set; } = new List<Student>();

        /// <summary>Current free-text search term (matches StudentNumber, First/Last name, Email or Course).</summary>
        public string? SearchTerm { get; set; }

        /// <summary>Column currently being sorted on.</summary>
        public string SortColumn { get; set; } = "LastName";

        /// <summary>"asc" or "desc".</summary>
        public string SortDirection { get; set; } = "asc";

        public int CurrentPage { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        /// <summary>Total number of records matching the current search (before paging is applied).</summary>
        public int TotalRecords { get; set; }

        public int TotalPages => (int)Math.Ceiling(TotalRecords / (double)PageSize);

        public bool HasPreviousPage => CurrentPage > 1;

        public bool HasNextPage => CurrentPage < TotalPages;

        /// <summary>Convenience helper used by the view to compute the opposite sort direction for column headers.</summary>
        public string OppositeSortDirection(string column) =>
            (SortColumn == column && SortDirection == "asc") ? "desc" : "asc";
    }
}

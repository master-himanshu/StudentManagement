namespace StudentManagement.Models
{
    /// <summary>
    /// Simple model bound to the generic Error view (Views/Shared/Error.cshtml).
    /// </summary>
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        /// <summary>
        /// Optional human-readable message describing what went wrong.
        /// Populated by the global exception handler middleware.
        /// </summary>
        public string? Message { get; set; }
    }
}

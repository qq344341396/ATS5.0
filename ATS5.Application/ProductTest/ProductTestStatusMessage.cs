using System;

namespace ATS5.Application.ProductTest
{
    /// <summary>
    /// Represents a UI-safe ProductTest status message projected from legacy callbacks.
    /// </summary>
    public sealed class ProductTestStatusMessage
    {
        /// <summary>
        /// Gets the local timestamp when the status projection was created.
        /// </summary>
        public DateTime CreatedAt { get; }

        /// <summary>
        /// Gets a value indicating whether the legacy message represents success.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Gets the legacy message text.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductTestStatusMessage"/> class.
        /// </summary>
        /// <param name="isSuccess">Whether the legacy message represents success.</param>
        /// <param name="message">Legacy message text.</param>
        public ProductTestStatusMessage(bool isSuccess, string message)
        {
            IsSuccess = isSuccess;
            Message = message ?? string.Empty;
            CreatedAt = DateTime.Now;
        }
    }
}

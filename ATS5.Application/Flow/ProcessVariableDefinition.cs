namespace ATS5.Application.Flow
{
    public sealed class ProcessVariableDefinition
    {
        public string TempName { get; set; } = string.Empty;

        public string TempValue { get; set; } = string.Empty;

        public string? TempUnit { get; set; } = string.Empty;

        public string? TempRemark { get; set; } = string.Empty;

        public string TempDataType { get; set; } = string.Empty;

        public string TempLength { get; set; } = string.Empty;
    }
}

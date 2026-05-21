namespace ATS5.Application.Flow
{
    public sealed class ProcessOutputDefinition
    {
        public string TestName { get; set; } = string.Empty;

        public string VarName { get; set; } = string.Empty;

        public string DataType { get; set; } = string.Empty;

        public string? Length { get; set; } = string.Empty;

        public string? Unit { get; set; } = string.Empty;

        public string MinLimit { get; set; } = string.Empty;

        public string MaxLimit { get; set; } = string.Empty;

        public bool IsEnabled { get; set; } = true;

        public string? ComparisonOperator { get; set; } = string.Empty;
    }
}

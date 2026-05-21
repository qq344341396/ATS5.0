namespace ATS5.Application.DataQuery
{
    public sealed class TestDetailRecord
    {
        public string TestName { get; set; } = string.Empty;

        public string VarName { get; set; } = string.Empty;

        public string DataType { get; set; } = string.Empty;

        public string MaxLimit { get; set; } = string.Empty;

        public string MinLimit { get; set; } = string.Empty;

        public string ComparisonOperator { get; set; } = string.Empty;

        public ushort ProjectIndex { get; set; }

        public string TestResult { get; set; } = string.Empty;

        public string ProjectTestResult { get; set; } = string.Empty;

        public string TestValue { get; set; } = string.Empty;

        public string Unit { get; set; } = string.Empty;

        public string ProjectName { get; set; } = string.Empty;

        public double TestTime { get; set; }
    }
}

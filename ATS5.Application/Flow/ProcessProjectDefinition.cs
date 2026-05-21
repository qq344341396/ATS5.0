using System.Collections.Generic;

namespace ATS5.Application.Flow
{
    public sealed class ProcessProjectDefinition
    {
        public bool IsEnable { get; set; }

        public string ProjectName { get; set; } = string.Empty;

        public string Script { get; set; } = string.Empty;

        public IList<ProcessOutputDefinition> Outputs { get; } = new List<ProcessOutputDefinition>();

        public IList<ProcessVariableDefinition> TempVariables { get; } = new List<ProcessVariableDefinition>();
    }
}

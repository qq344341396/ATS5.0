using System.Collections.Generic;

namespace ATS5.Application.Flow
{
    public sealed class ProcessFlowDefinition
    {
        public string DevCfgName { get; set; } = string.Empty;

        public string? MesParamName { get; set; } = string.Empty;

        public string Reserve2 { get; set; } = string.Empty;

        public IList<string> UdsFiles { get; } = new List<string>();

        public IList<string> DbcFiles { get; } = new List<string>();

        public IList<ProcessProjectDefinition> Projects { get; } = new List<ProcessProjectDefinition>();

        public IList<ProcessVariableDefinition> GlobalVariables { get; } = new List<ProcessVariableDefinition>();
    }
}

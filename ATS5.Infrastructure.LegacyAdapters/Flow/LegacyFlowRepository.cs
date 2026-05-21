using System;
using System.IO;
using System.Linq;
using ATS5.Application.Flow;
using ATS5.Infrastructure.LegacyAdapters.Runtime;
using ATSCommon;
using ATSCore;
using ATSModel;

namespace ATS5.Infrastructure.LegacyAdapters.Flow
{
    public sealed class LegacyFlowRepository : IFlowRepository
    {
        private readonly LegacyRuntimeContext _runtimeContext;

        public LegacyFlowRepository(LegacyRuntimeContext runtimeContext)
        {
            _runtimeContext = runtimeContext;
        }

        public FlowOpenResult Open(string flowName)
        {
            return _runtimeContext.Execute(
                () =>
                {
                    var path = $"{SysCache.PathFlows}{flowName}.fw";
                    if (!File.Exists(path))
                    {
                        return FlowOpenResult.Fail($"打开流程文件异常：{flowName}.fw");
                    }

                    var json = JsonHelper.GetJsonFile(path);
                    var legacyFlow = json.ToObject<ATSModel.Flow>();
                    if (legacyFlow == null)
                    {
                        return FlowOpenResult.Fail($"打开流程文件异常：{flowName}.fw");
                    }

                    return FlowOpenResult.Success(flowName, ToDefinition(legacyFlow), JsonHelper.ObjToJson(legacyFlow));
                });
        }

        public bool ExistsDeviceConfig(string deviceConfigName)
        {
            return _runtimeContext.Execute(() => File.Exists($"{SysCache.PathDevCfg}{deviceConfigName}.dev"));
        }

        public bool DeviceConfigHasContent(string deviceConfigName)
        {
            return _runtimeContext.Execute(
                () =>
                {
                    var path = $"{SysCache.PathDevCfg}{deviceConfigName}.dev";
                    return File.Exists(path) && new FileInfo(path).Length > 0;
                });
        }

        public string Serialize(ProcessFlowDefinition flow)
        {
            return _runtimeContext.Execute(() => JsonHelper.ObjToJson(ToLegacyFlow(flow)));
        }

        public void WriteFlow(string flowName, ProcessFlowDefinition flow)
        {
            _runtimeContext.Execute(() => JsonHelper.WriteJsonFile(ToLegacyFlow(flow), $"{SysCache.PathFlows}{flowName}.fw"));
        }

        public void WriteBackup(string flowName, string json)
        {
            _runtimeContext.Execute(() => JsonHelper.WriteJsonFile($"{SysCache.PathFlows}/BackupFlow/{flowName}-{DateTime.Now:yyyy-MM-dd}.fw", json));
        }

        public string CreateSampleCopyName(string sourceFlowName)
        {
            return $"{sourceFlowName}-WpfSample-{DateTime.Now:yyyyMMddHHmmss}";
        }

        internal static ProcessFlowDefinition ToDefinition(ATSModel.Flow source)
        {
            var result = new ProcessFlowDefinition
            {
                DevCfgName = source.DevCfgName ?? string.Empty,
                MesParamName = source.MESParamName,
                Reserve2 = source.Reserve2 ?? string.Empty
            };

            foreach (var udsFile in source.LstUdsFile)
            {
                result.UdsFiles.Add(udsFile.UdsFileName ?? string.Empty);
            }

            foreach (var dbcFile in source.LstDbcFile)
            {
                result.DbcFiles.Add(dbcFile.DbcFileName ?? string.Empty);
            }

            foreach (var project in source.LstProject)
            {
                var projectDefinition = new ProcessProjectDefinition
                {
                    IsEnable = project.IsEnable,
                    ProjectName = project.ProjectName ?? string.Empty,
                    Script = project.Script ?? string.Empty
                };

                foreach (var output in project.LstOutput)
                {
                    projectDefinition.Outputs.Add(new ProcessOutputDefinition
                    {
                        TestName = output.TestName ?? string.Empty,
                        VarName = output.VarName ?? string.Empty,
                        DataType = output.DataType ?? string.Empty,
                        Length = output.Length,
                        Unit = output.Unit,
                        MinLimit = output.MinLimit ?? string.Empty,
                        MaxLimit = output.MaxLimit ?? string.Empty,
                        IsEnabled = output.IsEnabled,
                        ComparisonOperator = output.ComparisonOperator
                    });
                }

                foreach (var tempVar in project.LstTempVar)
                {
                    projectDefinition.TempVariables.Add(ToVariableDefinition(tempVar));
                }

                result.Projects.Add(projectDefinition);
            }

            foreach (var globalVar in source.LstGlobalVar)
            {
                result.GlobalVariables.Add(ToVariableDefinition(globalVar));
            }

            return result;
        }

        internal static ATSModel.Flow ToLegacyFlow(ProcessFlowDefinition source)
        {
            var result = new ATSModel.Flow
            {
                DevCfgName = source.DevCfgName,
                MESParamName = source.MesParamName,
                Reserve2 = source.Reserve2
            };

            foreach (var udsFile in source.UdsFiles)
            {
                result.LstUdsFile.Add(new UdsFile { UdsFileName = udsFile });
            }

            foreach (var dbcFile in source.DbcFiles)
            {
                result.LstDbcFile.Add(new DbcFile { DbcFileName = dbcFile });
            }

            foreach (var project in source.Projects)
            {
                result.LstProject.Add(ToLegacyProject(project));
            }

            foreach (var globalVar in source.GlobalVariables)
            {
                result.LstGlobalVar.Add(ToLegacyVariable(globalVar));
            }

            return result;
        }

        internal static Project ToLegacyProject(ProcessProjectDefinition source)
        {
            var legacyProject = new Project
            {
                IsEnable = source.IsEnable,
                ProjectName = source.ProjectName,
                Script = source.Script
            };

            foreach (var output in source.Outputs)
            {
                legacyProject.LstOutput.Add(new Output
                {
                    TestName = output.TestName,
                    VarName = output.VarName,
                    DataType = output.DataType,
                    Length = output.Length,
                    Unit = output.Unit,
                    MinLimit = output.MinLimit,
                    MaxLimit = output.MaxLimit,
                    IsEnabled = output.IsEnabled,
                    ComparisonOperator = output.ComparisonOperator
                });
            }

            foreach (var tempVar in source.TempVariables)
            {
                legacyProject.LstTempVar.Add(ToLegacyVariable(tempVar));
            }

            return legacyProject;
        }

        internal static TempVar ToLegacyVariable(ProcessVariableDefinition source)
        {
            return new TempVar
            {
                TempName = source.TempName,
                TempValue = source.TempValue,
                TempUnit = source.TempUnit,
                TempRemark = source.TempRemark,
                TempDataType = source.TempDataType,
                TempLength = source.TempLength
            };
        }

        private static ProcessVariableDefinition ToVariableDefinition(TempVar source)
        {
            return new ProcessVariableDefinition
            {
                TempName = source.TempName ?? string.Empty,
                TempValue = source.TempValue ?? string.Empty,
                TempUnit = source.TempUnit,
                TempRemark = source.TempRemark,
                TempDataType = source.TempDataType ?? string.Empty,
                TempLength = source.TempLength ?? string.Empty
            };
        }

    }
}

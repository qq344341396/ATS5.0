using System;
using System.IO;
using System.Linq;
using System.Text;
using ATS5.Application.Flow;
using ATS5.Infrastructure.LegacyAdapters.Runtime;
using ATSCommon;
using ATSCore;
using ATSModel;

namespace ATS5.Infrastructure.LegacyAdapters.Flow
{
    public sealed class LegacyFlowScriptValidator : IFlowScriptValidator
    {
        private readonly LegacyRuntimeContext _runtimeContext;

        public LegacyFlowScriptValidator(LegacyRuntimeContext runtimeContext)
        {
            _runtimeContext = runtimeContext;
        }

        public FlowScriptValidationResult CheckAll(ProcessFlowDefinition flow, string flowName)
        {
            return _runtimeContext.Execute(() => CheckAllCore(flow));
        }

        private static FlowScriptValidationResult CheckAllCore(ProcessFlowDefinition flow)
        {
            var enabledProjects = flow.Projects.Where(project => project.IsEnable).ToList();
            var duplicateNames = enabledProjects
                .GroupBy(project => project.ProjectName)
                .Where(group => group.Count() > 1)
                .Select(group => $"【{group.Key}】,")
                .ToList();
            if (duplicateNames.Count > 0)
            {
                return FlowScriptValidationResult.FailWithCompleteMessage($"测试项名称:{string.Concat(duplicateNames)}重复,请检查!");
            }

            foreach (var project in enabledProjects)
            {
                var validationMessage = ValidateProjectData(project);
                if (!string.IsNullOrEmpty(validationMessage))
                {
                    return FlowScriptValidationResult.FailWithCompleteMessage(validationMessage);
                }
            }

            var error = new StringBuilder();
            foreach (var project in enabledProjects)
            {
                var tmpFlow = new ATSModel.Flow { DevCfgName = flow.DevCfgName };
                foreach (var globalVar in flow.GlobalVariables)
                {
                    tmpFlow.LstGlobalVar.Add(LegacyFlowRepository.ToLegacyVariable(globalVar));
                }

                tmpFlow.LstProject.Add(LegacyFlowRepository.ToLegacyProject(project));

                var tempFlowName = Path.GetRandomFileName();
                var filePath = $"{SysCache.PathFlows}{tempFlowName}.fw";
                JsonHelper.WriteJsonFile(tmpFlow, filePath);
                try
                {
                    var core = new ProductTestCore(tempFlowName, null, 0);
                    var code = core.MakeUpCode();
                    core.CompileCode(code, true);
                }
                catch (Exception ex)
                {
                    error.Append($"测试项目【{project.ProjectName}】：{ex.Message}");
                }
                finally
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
            }

            return error.Length == 0
                ? FlowScriptValidationResult.Success()
                : FlowScriptValidationResult.Fail(error.ToString());
        }

        private static string ValidateProjectData(ProcessProjectDefinition project)
        {
            foreach (var output in project.Outputs)
            {
                if (!ValidateOutput(output, project, out var message))
                {
                    return message;
                }
            }

            foreach (var tempVariable in project.TempVariables)
            {
                if (!ValidateTempVariable(tempVariable, project, out var message))
                {
                    return message;
                }
            }

            return string.Empty;
        }

        private static bool ValidateOutput(ProcessOutputDefinition output, ProcessProjectDefinition project, out string message)
        {
            message = string.Empty;
            FillDefaultComparisonOperator(output);

            if (!output.IsEnabled)
            {
                return true;
            }

            if (!IsValidVariableName(output.VarName))
            {
                message = $"测试项【{project.ProjectName}】中输出项【{output.VarName}】的【底层命名】首字符必须是英文字母、下划线或符号@,变量名中不能包含空格、小数点以及各种符号！";
                return false;
            }

            if (IsDoubleDataType(output.DataType))
            {
                if (!double.TryParse(output.MinLimit, out var min))
                {
                    message = $"测试项【{project.ProjectName}】中输出项【{output.VarName}】的【最小值】非有效数字,请检查!";
                    return false;
                }

                if (!double.TryParse(output.MaxLimit, out var max))
                {
                    message = $"测试项【{project.ProjectName}】中输出项【{output.VarName}】的【最大值】非有效数字,请检查!";
                    return false;
                }

                if (max < min)
                {
                    message = $"测试项【{project.ProjectName}】中输出项【{output.VarName}】的【最大值】不能小于【最小值】,请检查!";
                    return false;
                }

                if (IsDoubleArrayDataType(output.DataType) && !double.TryParse(output.Length, out _))
                {
                    message = $"测试项【{project.ProjectName}】中输出项【{output.VarName}】的【数组长度】非有效数字,请检查!";
                    return false;
                }
            }
            else if (string.Equals(output.DataType, "string", StringComparison.Ordinal) && output.MaxLimit != output.MinLimit)
            {
                message = $"测试项【{project.ProjectName}】中输出项【{output.VarName}】的【最大值】不等于【最小值】,有可能影响最终判断结果,请检查!";
                return false;
            }

            var duplicateTempVariable = project.TempVariables.FirstOrDefault(variable => variable.TempName == output.VarName);
            if (duplicateTempVariable != null)
            {
                message = $"测试项【{project.ProjectName}】中输出项【{output.TestName}】的【底层命名】与临时变量【{duplicateTempVariable.TempRemark}】变量重复,请检查!";
                return false;
            }

            return true;
        }

        private static bool ValidateTempVariable(ProcessVariableDefinition tempVariable, ProcessProjectDefinition project, out string message)
        {
            message = string.Empty;
            if (!IsValidVariableName(tempVariable.TempName))
            {
                message = $"测试项【{project.ProjectName}】中临时变量【{tempVariable.TempName}】的【底层命名】首字符必须是英文字母、下划线或符号@,变量名中不能包含空格、小数点以及各种符号！";
                return false;
            }

            if (IsDoubleDataType(tempVariable.TempDataType))
            {
                if (!double.TryParse(tempVariable.TempValue, out _))
                {
                    message = $"测试项【{project.ProjectName}】中临时变量【{tempVariable.TempName}】的【值】非有效数字,请检查!";
                    return false;
                }

                if (IsDoubleArrayDataType(tempVariable.TempDataType) && !double.TryParse(tempVariable.TempLength, out _))
                {
                    message = $"测试项【{project.ProjectName}】中临时变量【{tempVariable.TempName}】的【数组长度】非有效数字,请检查!";
                    return false;
                }
            }

            var duplicateOutput = project.Outputs.FirstOrDefault(output => output.VarName == tempVariable.TempName);
            if (duplicateOutput != null)
            {
                message = $"测试项【{project.ProjectName}】中临时变量【{tempVariable.TempRemark}】的【底层命名】与输出项【{duplicateOutput.TestName}】重复,请检查!";
                return false;
            }

            return true;
        }

        private static bool IsValidVariableName(string name)
        {
            return !string.IsNullOrEmpty(name) && Checker.IsValidVariableName(name);
        }

        private static void FillDefaultComparisonOperator(ProcessOutputDefinition output)
        {
            if (!string.IsNullOrEmpty(output.ComparisonOperator))
            {
                return;
            }

            if (string.Equals(output.DataType, "string", StringComparison.Ordinal)
                || string.Equals(output.DataType, "var", StringComparison.Ordinal))
            {
                output.ComparisonOperator = "==,&&,==";
            }
            else if (IsDoubleDataType(output.DataType))
            {
                output.ComparisonOperator = ">=,&&,<=";
            }
        }

        private static bool IsDoubleDataType(string dataType)
        {
            return string.Equals(dataType, "double", StringComparison.Ordinal)
                || IsDoubleArrayDataType(dataType);
        }

        private static bool IsDoubleArrayDataType(string dataType)
        {
            return string.Equals(dataType, "double[]", StringComparison.Ordinal);
        }
    }
}

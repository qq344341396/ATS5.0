using ATS5.Application.Flow;
using ATS5.Infrastructure.LegacyAdapters.Flow;
using ATS5.Infrastructure.LegacyAdapters.Runtime;
using Xunit;

namespace ATS5.Tests.Flow
{
    public sealed class LegacyFlowScriptValidatorTests
    {
        [Fact]
        public void CheckAll_ReturnsCompleteLegacyMessage_WhenEnabledProjectNamesDuplicate()
        {
            var validator = new LegacyFlowScriptValidator(
                new LegacyRuntimeContext(
                    new DebugRuntimePathProvider(@"D:\CODE\ATE\01_code\ATS\bin\Debug")));
            var flow = new ProcessFlowDefinition { DevCfgName = "DeviceA" };
            flow.Projects.Add(new ProcessProjectDefinition { IsEnable = true, ProjectName = "P1" });
            flow.Projects.Add(new ProcessProjectDefinition { IsEnable = true, ProjectName = "P1" });
            flow.Projects.Add(new ProcessProjectDefinition { IsEnable = false, ProjectName = "P1" });

            var result = validator.CheckAll(flow, "Sample");

            Assert.False(result.IsSuccess);
            Assert.True(result.IsUserMessageComplete);
            Assert.Equal("测试项名称:【P1】,重复,请检查!", result.Message);
        }

        [Fact]
        public void CheckAll_ReturnsCompleteLegacyMessage_WhenOutputNameDuplicatesTempVariable()
        {
            var validator = CreateValidator();
            var flow = CreateFlow();
            var project = CreateEnabledProject();
            project.Outputs.Add(new ProcessOutputDefinition
            {
                TestName = "Out1",
                VarName = "sameName",
                DataType = "string",
                MinLimit = "OK",
                MaxLimit = "OK"
            });
            project.TempVariables.Add(new ProcessVariableDefinition
            {
                TempName = "sameName",
                TempRemark = "Temp1",
                TempDataType = "string"
            });
            flow.Projects.Add(project);

            var result = validator.CheckAll(flow, "Sample");

            Assert.False(result.IsSuccess);
            Assert.True(result.IsUserMessageComplete);
            Assert.Equal("测试项【P1】中输出项【Out1】的【底层命名】与临时变量【Temp1】变量重复,请检查!", result.Message);
        }

        [Fact]
        public void CheckAll_ReturnsCompleteLegacyMessage_WhenOutputVariableNameInvalid()
        {
            var validator = CreateValidator();
            var flow = CreateFlow();
            var project = CreateEnabledProject();
            project.Outputs.Add(new ProcessOutputDefinition
            {
                TestName = "Out1",
                VarName = "1bad",
                DataType = "double",
                MinLimit = "0",
                MaxLimit = "1"
            });
            flow.Projects.Add(project);

            var result = validator.CheckAll(flow, "Sample");

            Assert.False(result.IsSuccess);
            Assert.True(result.IsUserMessageComplete);
            Assert.Equal("测试项【P1】中输出项【1bad】的【底层命名】首字符必须是英文字母、下划线或符号@,变量名中不能包含空格、小数点以及各种符号！", result.Message);
        }

        [Fact]
        public void CheckAll_ReturnsCompleteLegacyMessage_WhenTempVariableDoubleValueInvalid()
        {
            var validator = CreateValidator();
            var flow = CreateFlow();
            var project = CreateEnabledProject();
            project.TempVariables.Add(new ProcessVariableDefinition
            {
                TempName = "temp1",
                TempValue = "bad",
                TempDataType = "double"
            });
            flow.Projects.Add(project);

            var result = validator.CheckAll(flow, "Sample");

            Assert.False(result.IsSuccess);
            Assert.True(result.IsUserMessageComplete);
            Assert.Equal("测试项【P1】中临时变量【temp1】的【值】非有效数字,请检查!", result.Message);
        }

        [Fact]
        public void CheckAll_ReturnsCompleteLegacyMessage_WhenOutputDoubleArrayLengthInvalid()
        {
            var validator = CreateValidator();
            var flow = CreateFlow();
            var project = CreateEnabledProject();
            project.Outputs.Add(new ProcessOutputDefinition
            {
                TestName = "Out1",
                VarName = "out1",
                DataType = "double[]",
                MinLimit = "0",
                MaxLimit = "1",
                Length = "bad"
            });
            flow.Projects.Add(project);

            var result = validator.CheckAll(flow, "Sample");

            Assert.False(result.IsSuccess);
            Assert.True(result.IsUserMessageComplete);
            Assert.Equal("测试项【P1】中输出项【out1】的【数组长度】非有效数字,请检查!", result.Message);
        }

        [Fact]
        public void CheckAll_FillsEmptyComparisonOperatorsBeforeDeterministicValidation()
        {
            var validator = CreateValidator();
            var flow = CreateFlow();
            var project = CreateEnabledProject();
            var stringOutput = CreateOutput("StringOut", "stringOut", "string");
            var varOutput = CreateOutput("VarOut", "varOut", "var");
            var doubleOutput = CreateOutput("DoubleOut", "doubleOut", "double");
            var doubleArrayOutput = CreateOutput("DoubleArrayOut", "doubleArrayOut", "double[]");
            project.Outputs.Add(stringOutput);
            project.Outputs.Add(varOutput);
            project.Outputs.Add(doubleOutput);
            project.Outputs.Add(doubleArrayOutput);
            project.TempVariables.Add(new ProcessVariableDefinition
            {
                TempName = "temp1",
                TempValue = "bad",
                TempDataType = "double"
            });
            flow.Projects.Add(project);

            var result = validator.CheckAll(flow, "Sample");

            Assert.False(result.IsSuccess);
            Assert.Equal("==,&&,==", stringOutput.ComparisonOperator);
            Assert.Equal("==,&&,==", varOutput.ComparisonOperator);
            Assert.Equal(">=,&&,<=", doubleOutput.ComparisonOperator);
            Assert.Equal(">=,&&,<=", doubleArrayOutput.ComparisonOperator);
            Assert.Equal("测试项【P1】中临时变量【temp1】的【值】非有效数字,请检查!", result.Message);
        }

        [Fact]
        public void CheckAll_FillsEmptyComparisonOperator_ForDisabledOutputsBeforeSkippingValidation()
        {
            var validator = CreateValidator();
            var flow = CreateFlow();
            var project = CreateEnabledProject();
            var disabledStringOutput = CreateOutput("StringOut", "stringOut", "string");
            var disabledDoubleOutput = CreateOutput("DoubleOut", "doubleOut", "double");
            disabledStringOutput.IsEnabled = false;
            disabledDoubleOutput.IsEnabled = false;
            project.Outputs.Add(disabledStringOutput);
            project.Outputs.Add(disabledDoubleOutput);
            project.TempVariables.Add(new ProcessVariableDefinition
            {
                TempName = "temp1",
                TempValue = "bad",
                TempDataType = "double"
            });
            flow.Projects.Add(project);

            var result = validator.CheckAll(flow, "Sample");

            Assert.False(result.IsSuccess);
            Assert.Equal("==,&&,==", disabledStringOutput.ComparisonOperator);
            Assert.Equal(">=,&&,<=", disabledDoubleOutput.ComparisonOperator);
            Assert.Equal("测试项【P1】中临时变量【temp1】的【值】非有效数字,请检查!", result.Message);
        }

        private static LegacyFlowScriptValidator CreateValidator()
        {
            return new LegacyFlowScriptValidator(
                new LegacyRuntimeContext(
                    new DebugRuntimePathProvider(@"D:\CODE\ATE\01_code\ATS\bin\Debug")));
        }

        private static ProcessFlowDefinition CreateFlow()
        {
            return new ProcessFlowDefinition { DevCfgName = "DeviceA" };
        }

        private static ProcessProjectDefinition CreateEnabledProject()
        {
            return new ProcessProjectDefinition
            {
                IsEnable = true,
                ProjectName = "P1",
                Script = "var x = 1;"
            };
        }

        private static ProcessOutputDefinition CreateOutput(string testName, string varName, string dataType)
        {
            return new ProcessOutputDefinition
            {
                TestName = testName,
                VarName = varName,
                DataType = dataType,
                MinLimit = "1",
                MaxLimit = "1",
                Length = "1",
                ComparisonOperator = string.Empty
            };
        }
    }
}

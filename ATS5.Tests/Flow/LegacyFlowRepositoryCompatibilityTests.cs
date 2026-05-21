using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using ATS5.Application.Flow;
using ATS5.Infrastructure.LegacyAdapters.Flow;
using ATS5.Infrastructure.LegacyAdapters.Runtime;
using Xunit;

namespace ATS5.Tests.Flow
{
    [Trait("Category", "LegacyIntegration")]
    [Trait("Category", "FlowParity")]
    public sealed class LegacyFlowRepositoryCompatibilityTests
    {
        private const string LegacyRuntimeRoot = @"D:\CODE\ATE\01_code\ATS\bin\Debug";

        private static readonly string[] PreferredSamples =
        {
            "测试1",
            "测试2",
            "超充重卡PDU-临工（三支路）",
            "大秦800Y-BMS",
            "徐工528子母车 扩展帧-EOL测试",
            "DCDC-EOL测试",
            "Maserati M189 成品测试（主控）-通道2"
        };

        [Fact]
        public void Open_MapsRealDebugFlowCorpus_ToLegacyFlowSemantics()
        {
            var samples = GetExistingSamples();
            var repository = CreateRepository();
            var legacyAccess = LegacyAccess.Instance;

            foreach (var sample in samples)
            {
                var legacyFlow = legacyAccess.LoadFlow(sample);
                var result = repository.Open(sample);

                Assert.True(result.IsSuccess, $"{sample}: {result.Message}");
                Assert.Equal(sample, result.FlowName);
                AssertFlowDefinitionMatchesLegacy(sample, legacyFlow, result.Flow);
            }
        }

        [Fact]
        public void Serialize_RoundTripsRealDebugFlowCorpus_ToLegacyFlowSemantics()
        {
            var samples = GetExistingSamples();
            var repository = CreateRepository();
            var legacyAccess = LegacyAccess.Instance;

            foreach (var sample in samples)
            {
                var legacyFlow = legacyAccess.LoadFlow(sample);
                var openResult = repository.Open(sample);
                Assert.True(openResult.IsSuccess, $"{sample}: {openResult.Message}");

                var serialized = repository.Serialize(openResult.Flow);
                var roundTripped = legacyAccess.ToFlow(serialized);

                Assert.NotNull(roundTripped);
                AssertLegacyFlowMatches(sample, legacyFlow, roundTripped);
            }
        }

        private static LegacyFlowRepository CreateRepository()
        {
            return new LegacyFlowRepository(
                new LegacyRuntimeContext(
                    new DebugRuntimePathProvider(LegacyRuntimeRoot)));
        }

        private static IReadOnlyList<string> GetExistingSamples()
        {
            var flowsDirectory = Path.Combine(LegacyRuntimeRoot, "SysCache", "Flows");
            Assert.True(Directory.Exists(flowsDirectory), $"真实 Debug 流程目录不存在：{flowsDirectory}");

            var samples = PreferredSamples
                .Where(sample => File.Exists(Path.Combine(flowsDirectory, sample + ".fw")))
                .ToList();

            Assert.NotEmpty(samples);
            Assert.Equal(PreferredSamples.Length, samples.Count);
            return samples;
        }

        private static void AssertFlowDefinitionMatchesLegacy(string sample, object expected, ProcessFlowDefinition actual)
        {
            Assert.Equal(GetString(expected, "DevCfgName"), actual.DevCfgName);
            Assert.Equal(GetString(expected, "MESParamName"), actual.MesParamName);
            Assert.Equal(GetString(expected, "Reserve2"), actual.Reserve2);
            Assert.Equal(
                GetLegacyNameList(expected, "LstDbcFile", "DbcFileName"),
                actual.DbcFiles);
            Assert.Equal(
                GetLegacyNameList(expected, "LstUdsFile", "UdsFileName"),
                actual.UdsFiles);
            var expectedProjects = GetObjectList(expected, "LstProject");
            var expectedGlobalVariables = GetObjectList(expected, "LstGlobalVar");
            Assert.Equal(expectedProjects.Count, actual.Projects.Count);
            Assert.Equal(expectedGlobalVariables.Count, actual.GlobalVariables.Count);

            for (var i = 0; i < expectedProjects.Count; i++)
            {
                AssertProjectDefinitionMatchesLegacy($"{sample}/Project[{i}]", expectedProjects[i], actual.Projects[i]);
            }

            for (var i = 0; i < expectedGlobalVariables.Count; i++)
            {
                AssertVariableDefinitionMatchesLegacy($"{sample}/GlobalVariables[{i}]", expectedGlobalVariables[i], actual.GlobalVariables[i]);
            }
        }

        private static void AssertProjectDefinitionMatchesLegacy(string context, object expected, ProcessProjectDefinition actual)
        {
            Assert.Equal(GetBool(expected, "IsEnable"), actual.IsEnable);
            Assert.Equal(GetString(expected, "ProjectName"), actual.ProjectName);
            Assert.Equal(GetString(expected, "Script"), actual.Script);
            var expectedOutputs = GetObjectList(expected, "LstOutput");
            var expectedTempVariables = GetObjectList(expected, "LstTempVar");
            Assert.Equal(expectedOutputs.Count, actual.Outputs.Count);
            Assert.Equal(expectedTempVariables.Count, actual.TempVariables.Count);

            for (var i = 0; i < expectedOutputs.Count; i++)
            {
                AssertOutputDefinitionMatchesLegacy($"{context}/Outputs[{i}]", expectedOutputs[i], actual.Outputs[i]);
            }

            for (var i = 0; i < expectedTempVariables.Count; i++)
            {
                AssertVariableDefinitionMatchesLegacy($"{context}/TempVariables[{i}]", expectedTempVariables[i], actual.TempVariables[i]);
            }
        }

        private static void AssertOutputDefinitionMatchesLegacy(string context, object expected, ProcessOutputDefinition actual)
        {
            Assert.Equal(GetString(expected, "TestName"), actual.TestName);
            Assert.Equal(GetString(expected, "VarName"), actual.VarName);
            Assert.Equal(GetString(expected, "DataType"), actual.DataType);
            Assert.Equal(GetString(expected, "Length"), actual.Length);
            Assert.Equal(GetString(expected, "Unit"), actual.Unit);
            Assert.Equal(GetString(expected, "MinLimit"), actual.MinLimit);
            Assert.Equal(GetString(expected, "MaxLimit"), actual.MaxLimit);
            Assert.Equal(GetBool(expected, "IsEnabled"), actual.IsEnabled);
            Assert.Equal(GetString(expected, "ComparisonOperator"), actual.ComparisonOperator);
        }

        private static void AssertVariableDefinitionMatchesLegacy(string context, object expected, ProcessVariableDefinition actual)
        {
            Assert.Equal(GetString(expected, "TempName"), actual.TempName);
            Assert.Equal(GetString(expected, "TempValue"), actual.TempValue);
            Assert.Equal(GetString(expected, "TempUnit"), actual.TempUnit);
            Assert.Equal(GetString(expected, "TempRemark"), actual.TempRemark);
            Assert.Equal(GetString(expected, "TempDataType"), actual.TempDataType);
            Assert.Equal(GetString(expected, "TempLength"), actual.TempLength);
        }

        private static void AssertLegacyFlowMatches(string sample, object expected, object actual)
        {
            Assert.Equal(GetString(expected, "DevCfgName"), GetString(actual, "DevCfgName"));
            Assert.Equal(GetString(expected, "MESParamName"), GetString(actual, "MESParamName"));
            Assert.Equal(GetString(expected, "Reserve2"), GetString(actual, "Reserve2"));
            Assert.Equal(
                GetLegacyNameList(expected, "LstDbcFile", "DbcFileName"),
                GetLegacyNameList(actual, "LstDbcFile", "DbcFileName"));
            Assert.Equal(
                GetLegacyNameList(expected, "LstUdsFile", "UdsFileName"),
                GetLegacyNameList(actual, "LstUdsFile", "UdsFileName"));
            var expectedProjects = GetObjectList(expected, "LstProject");
            var actualProjects = GetObjectList(actual, "LstProject");
            var expectedGlobalVariables = GetObjectList(expected, "LstGlobalVar");
            var actualGlobalVariables = GetObjectList(actual, "LstGlobalVar");
            Assert.Equal(expectedProjects.Count, actualProjects.Count);
            Assert.Equal(expectedGlobalVariables.Count, actualGlobalVariables.Count);

            for (var i = 0; i < expectedProjects.Count; i++)
            {
                AssertLegacyProjectMatches($"{sample}/Project[{i}]", expectedProjects[i], actualProjects[i]);
            }

            for (var i = 0; i < expectedGlobalVariables.Count; i++)
            {
                AssertLegacyVariableMatches($"{sample}/GlobalVariables[{i}]", expectedGlobalVariables[i], actualGlobalVariables[i]);
            }
        }

        private static void AssertLegacyProjectMatches(string context, object expected, object actual)
        {
            Assert.Equal(GetBool(expected, "IsEnable"), GetBool(actual, "IsEnable"));
            Assert.Equal(GetString(expected, "ProjectName"), GetString(actual, "ProjectName"));
            Assert.Equal(GetString(expected, "Script"), GetString(actual, "Script"));
            var expectedOutputs = GetObjectList(expected, "LstOutput");
            var actualOutputs = GetObjectList(actual, "LstOutput");
            var expectedTempVariables = GetObjectList(expected, "LstTempVar");
            var actualTempVariables = GetObjectList(actual, "LstTempVar");
            Assert.Equal(expectedOutputs.Count, actualOutputs.Count);
            Assert.Equal(expectedTempVariables.Count, actualTempVariables.Count);

            for (var i = 0; i < expectedOutputs.Count; i++)
            {
                AssertLegacyOutputMatches($"{context}/Outputs[{i}]", expectedOutputs[i], actualOutputs[i]);
            }

            for (var i = 0; i < expectedTempVariables.Count; i++)
            {
                AssertLegacyVariableMatches($"{context}/TempVariables[{i}]", expectedTempVariables[i], actualTempVariables[i]);
            }
        }

        private static void AssertLegacyOutputMatches(string context, object expected, object actual)
        {
            Assert.Equal(GetString(expected, "TestName"), GetString(actual, "TestName"));
            Assert.Equal(GetString(expected, "VarName"), GetString(actual, "VarName"));
            Assert.Equal(GetString(expected, "DataType"), GetString(actual, "DataType"));
            Assert.Equal(GetString(expected, "Length"), GetString(actual, "Length"));
            Assert.Equal(GetString(expected, "Unit"), GetString(actual, "Unit"));
            Assert.Equal(GetString(expected, "MinLimit"), GetString(actual, "MinLimit"));
            Assert.Equal(GetString(expected, "MaxLimit"), GetString(actual, "MaxLimit"));
            Assert.Equal(GetBool(expected, "IsEnabled"), GetBool(actual, "IsEnabled"));
            Assert.Equal(GetString(expected, "ComparisonOperator"), GetString(actual, "ComparisonOperator"));
        }

        private static void AssertLegacyVariableMatches(string context, object expected, object actual)
        {
            Assert.Equal(GetString(expected, "TempName"), GetString(actual, "TempName"));
            Assert.Equal(GetString(expected, "TempValue"), GetString(actual, "TempValue"));
            Assert.Equal(GetString(expected, "TempUnit"), GetString(actual, "TempUnit"));
            Assert.Equal(GetString(expected, "TempRemark"), GetString(actual, "TempRemark"));
            Assert.Equal(GetString(expected, "TempDataType"), GetString(actual, "TempDataType"));
            Assert.Equal(GetString(expected, "TempLength"), GetString(actual, "TempLength"));
        }

        private static IReadOnlyList<string?> GetLegacyNameList(object source, string listPropertyName, string namePropertyName)
        {
            return GetObjectList(source, listPropertyName)
                .Select(item => GetString(item, namePropertyName))
                .ToList();
        }

        private static IReadOnlyList<object> GetObjectList(object source, string propertyName)
        {
            var value = GetValue(source, propertyName);
            if (value == null)
            {
                return Array.Empty<object>();
            }

            return ((IEnumerable)value).Cast<object>().ToList();
        }

        private static string? GetString(object source, string propertyName)
        {
            return (string?)GetValue(source, propertyName);
        }

        private static bool GetBool(object source, string propertyName)
        {
            return (bool)GetValue(source, propertyName)!;
        }

        private static object? GetValue(object source, string propertyName)
        {
            var property = source.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
            Assert.NotNull(property);
            return property!.GetValue(source);
        }

        private sealed class LegacyAccess
        {
            private static readonly Lazy<LegacyAccess> Current = new Lazy<LegacyAccess>(Create);

            private readonly MethodInfo _getJsonFile;
            private readonly MethodInfo _toObject;
            private readonly Type _flowType;

            private LegacyAccess(MethodInfo getJsonFile, MethodInfo toObject, Type flowType)
            {
                _getJsonFile = getJsonFile;
                _toObject = toObject;
                _flowType = flowType;
            }

            public static LegacyAccess Instance => Current.Value;

            public object LoadFlow(string sample)
            {
                var path = Path.Combine(LegacyRuntimeRoot, "SysCache", "Flows", sample + ".fw");
                var json = (string?)_getJsonFile.Invoke(null, new object[] { path });
                Assert.False(string.IsNullOrEmpty(json), $"真实 Debug 流程文件为空：{path}");
                return ToFlow(json!);
            }

            public object ToFlow(string json)
            {
                var method = _toObject.MakeGenericMethod(_flowType);
                var flow = method.Invoke(null, new object[] { json });
                Assert.NotNull(flow);
                return flow!;
            }

            private static LegacyAccess Create()
            {
                AppDomain.CurrentDomain.AssemblyResolve += ResolveLegacyAssembly;

                var commonAssembly = LoadLegacyAssembly("ATSCommon");
                var modelAssembly = LoadLegacyAssembly("ATSModel");
                var jsonHelperType = commonAssembly.GetType("ATSCommon.JsonHelper", throwOnError: true)!;
                var getJsonFile = jsonHelperType.GetMethod("GetJsonFile", BindingFlags.Public | BindingFlags.Static)!;
                var toObject = jsonHelperType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Single(method => method.Name == "ToObject" && method.IsGenericMethodDefinition);
                var flowType = modelAssembly.GetType("ATSModel.Flow", throwOnError: true)!;

                return new LegacyAccess(getJsonFile, toObject, flowType);
            }

            private static Assembly LoadLegacyAssembly(string assemblyName)
            {
                var loaded = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(assembly => string.Equals(assembly.GetName().Name, assemblyName, StringComparison.OrdinalIgnoreCase));
                if (loaded != null)
                {
                    return loaded;
                }

                return Assembly.LoadFrom(Path.Combine(LegacyRuntimeRoot, assemblyName + ".dll"));
            }

            private static Assembly? ResolveLegacyAssembly(object sender, ResolveEventArgs args)
            {
                var assemblyName = new AssemblyName(args.Name).Name + ".dll";
                var searchPaths = new[]
                {
                    Path.Combine(LegacyRuntimeRoot, assemblyName),
                    Path.Combine(LegacyRuntimeRoot, "AppDll", assemblyName),
                    Path.Combine(LegacyRuntimeRoot, "AppDll", "sqlite", assemblyName)
                };

                var path = searchPaths.FirstOrDefault(File.Exists);
                return path == null ? null : Assembly.LoadFrom(path);
            }
        }
    }
}

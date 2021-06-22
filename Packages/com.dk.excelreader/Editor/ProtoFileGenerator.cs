using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Linq;
using Google.Protobuf;
using UnityEngine;

namespace DK.ExcelReader
{
    public class ProtoFileGenerator : CodeGeneratorBase
    {
        private const string _csharpNamespace = "Sorani.Yookoso.GameData";
        private const string _configManagerName = "ConfigManager";
        private const string _testFileName = "GeneratedDataTest";
        private const string _binExt = ".bytes";

        public override void GenerateBasicCode(string className, ExcelReaderSettings settings)
        {
            var classInfo = ReadExcelData(className + ".xlsx", settings);

            var sb = new StringBuilder();

            // header
            sb.AppendLine("syntax = \"proto3\";");
            sb.AppendLine($"option csharp_namespace=\"{_csharpNamespace}\";");
            sb.AppendLine("option go_package = \".;PbMessage\";");
            sb.AppendLine("");

            // message body
            sb.AppendLine($"message {classInfo.className}");
            sb.AppendLine("{");

            foreach (var property in classInfo.properties.Where(property => !string.IsNullOrEmpty(property.name)))
            {
                sb.AppendLine(
                    $"    {GetTypeProtoName(property.type)} {property.name} = {classInfo.properties.IndexOf(property) + 1};");
            }

            sb.AppendLine("}");
            sb.AppendLine("");

            //message container
            sb.AppendLine($"message {classInfo.className}Container");
            sb.AppendLine("{");
            sb.AppendLine($"    map<int32, {classInfo.className}> Container = 1;");
            sb.AppendLine("}");

            // output file
            var targetPath = Path.Combine(settings.protoPath, className + ".proto");
            using (var sw = new StreamWriter(targetPath))
            {
                sw.Write(sb);
            }

            // additional partial class file for global settings
            if (!className.Contains("Global")) return;

            var globalSb = new StringBuilder();

            // header
            globalSb.AppendLine("namespace Sorani.Yookoso.GameData");
            globalSb.AppendLine("{");
            globalSb.AppendLine($"    public partial class {className}");
            globalSb.AppendLine("    {");

            var idPropertyInfo = classInfo.properties.Find((t) => t.name.Equals("ID"));
            var namePropertyInfo = classInfo.properties.Find((t) => t.name.Equals("Enum"));

            if (namePropertyInfo == null || idPropertyInfo == null) return;
            var idValueList = idPropertyInfo.value as IList;
            var nameValueList = namePropertyInfo.value as IList;
            for (var i = 0; i < nameValueList.Count; ++i)
            {
                globalSb.AppendLine($"        public static readonly int {nameValueList[i]} = {idValueList[i]};");
            }

            globalSb.AppendLine("    }");
            globalSb.AppendLine("}");

            // output file
            var globalDefinePath = Path.Combine(settings.csharpPath, className + ".generated.cs");
            using (var sw = new StreamWriter(globalDefinePath))
            {
                sw.Write(globalSb);
            }
        }

#if UNITY_ANDROID
        public override void PostCodeGeneration(ExcelReaderSettings settings)
        {
            // generate csharp code based on proto file
            var processInfo = new ProcessStartInfo(Path.GetFullPath("Packages/com.dk.excelreader/tools/generate_csharp.bat"))
            {
                CreateNoWindow = true, UseShellExecute = false, WorkingDirectory = Application.dataPath
            };

            var exePath = Path.GetFullPath("Packages/com.dk.excelreader/tools/protoc.exe");
            var csharpPath = (Path.GetFullPath(settings.csharpPath)).Replace('\\', '/');
            var protoPath = (Path.GetFullPath(settings.protoPath)).Replace('\\', '/');
            processInfo.Arguments = $"{exePath} {csharpPath} {protoPath}";

            var process = new Process {StartInfo = processInfo};
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;

            process.Start();
            process.WaitForExit();
        }
#endif

#if UNITY_IOS
        public override void PostCodeGeneration(ExcelReaderSettings settings)
        {
            // generate csharp code based on proto file
            var processInfo = new ProcessStartInfo(Path.GetFullPath("Packages/com.dk.excelreader/tools/generate_csharp.sh"))
            {
                CreateNoWindow = true, UseShellExecute = false, WorkingDirectory = Application.dataPath
            };

            var exePath = "protoc";
            var csharpPath = (Path.GetFullPath(settings.csharpPath)).Replace('\\', '/');
            var protoPath = (Path.GetFullPath(settings.protoPath)).Replace('\\', '/');
            processInfo.Arguments = $"{exePath} {csharpPath} {protoPath}";

            var process = new Process {StartInfo = processInfo};
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;

            process.Start();
            process.WaitForExit();
        }
#endif

        public override void GenerateConfigManager(string[] classNames, ExcelReaderSettings settings)
        {
            var sb = new StringBuilder();

            // using
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine("using Sorani.Utils;");
            sb.AppendLine("using Sorani.ResourceManagement;");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using UnityEngine.AddressableAssets;");
            sb.AppendLine("using UnityEngine.ResourceManagement.AsyncOperations;");
            sb.AppendLine("using Google.Protobuf;");
            sb.AppendLine("");

            
            // init data
            var amount = classNames.Length;
            
            // class define
            sb.AppendLine("namespace Sorani.Yookoso.GameData");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {_configManagerName} : Utils.SingletonInstance<ConfigManager>, IAsyncLoading");
            sb.AppendLine("    {");
            sb.AppendLine("        private Dictionary<Type, IConfigContainer> dataDict;");
            sb.AppendLine("");
            sb.AppendLine("        public Task Unload(IProgress progress, float weight)");
            sb.AppendLine("        {");
            sb.AppendLine("            dataDict.Clear();");
            sb.AppendLine("            return Task.CompletedTask;");
            sb.AppendLine("        }");
            sb.AppendLine("");
            sb.AppendLine("        public void LoadConfig(TextAsset textAsset, Type configType, Type containerType)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (dataDict == null) dataDict = new Dictionary<Type, IConfigContainer>();");
            sb.AppendLine("            var parser = containerType.GetProperty(\"Parser\").GetValue(containerType);");
            sb.AppendLine("            var container = parser.GetType().GetMethod(\"ParseFrom\", new Type[] {typeof(byte[])}).Invoke(parser, new object[] {textAsset.bytes});");
            sb.AppendLine("            dataDict[configType] = (IConfigContainer)container;");
            sb.AppendLine("        }");
            sb.AppendLine("");
            sb.AppendLine("        public async Task BeginLoad(IProgress progress, float weight)");
            sb.AppendLine("        {");
            // init dict
            sb.AppendLine("            dataDict = new Dictionary<Type, IConfigContainer>();");
            sb.AppendLine($"            float step = 1.0f / {amount};");
            sb.AppendLine($"            var taskList = new List<Task>();");
            sb.AppendLine("");

            foreach (var path in classNames)
            {
                if (path.Contains("~$")) continue;

                var name = Path.GetFileNameWithoutExtension(path);
                var dataPath = Path.Combine(settings.binPath, name + _binExt).Replace("\\", "/");
                
                sb.AppendLine($"            taskList.Add(LoadConfig(\"{dataPath}\", typeof({name}), typeof({name}Container), progress, step, weight));");
            }
            
            sb.AppendLine("");
            // wait all task
            sb.AppendLine("            await Task.WhenAll(taskList);");

            sb.AppendLine("        }");
            sb.AppendLine("");
            sb.AppendLine("        private async Task LoadConfig(string filePath, Type configType, Type containerType, IProgress progress, float step, float weight)");
            sb.AppendLine("        {");
            sb.AppendLine("            var handle = Addressables.LoadAssetAsync<TextAsset>(filePath);");
            sb.AppendLine("            var textAsset = await handle.Task;");
            sb.AppendLine("");
            sb.AppendLine("            var parser = containerType.GetProperty(\"Parser\").GetValue(containerType);");
            sb.AppendLine("            var container = parser.GetType().GetMethod(\"ParseFrom\", new Type[] {typeof(byte[])}).Invoke(parser, new object[] {textAsset.bytes});");
            sb.AppendLine("");
            sb.AppendLine("            dataDict.Add(configType, (IConfigContainer)container);");
            sb.AppendLine("            progress.SetProgress(progress.Progress() + step * weight);");
            sb.AppendLine("        }");
            sb.AppendLine("");
            sb.AppendLine("        public T GetConfig<T>(int id)");
            sb.AppendLine("        {");
            sb.AppendLine("            try");
            sb.AppendLine("            {");
            sb.AppendLine("                return (T)dataDict[typeof(T)].GetConfig(id);");
            sb.AppendLine("            }");
            sb.AppendLine("            catch (SystemException e)");
            sb.AppendLine("            {");
            sb.AppendLine("                DKLog.LogError($\"Cannot find ID: {id} in {typeof(T).Name}\");");
            sb.AppendLine("                throw e;");
            sb.AppendLine("            }");
            sb.AppendLine("        }");

            sb.AppendLine("        public ICollection<T> GetContainer<T>()");
            sb.AppendLine("        {");
            sb.AppendLine("            return dataDict[typeof(T)].GetContainer<T>();");
            sb.AppendLine("        }");

            sb.AppendLine("    }");
            //

            // interface define
            sb.AppendLine("");
            sb.AppendLine($"    public interface IConfigContainer");
            sb.AppendLine("    {");
            sb.AppendLine($"        IMessage GetConfig(int id);");
            sb.AppendLine($"        ICollection<T> GetContainer<T>();");
            sb.AppendLine("    }");

            // container class extension
            foreach (var path in classNames)
            {
                if (path.Contains("~$")) continue;
                var name = Path.GetFileNameWithoutExtension(path);

                sb.AppendLine($"    public partial class {name}Container : IConfigContainer");
                sb.AppendLine("    {");
                sb.AppendLine($"        public IMessage GetConfig(int id)");
                sb.AppendLine("        {");
                sb.AppendLine("            return Container[id];");
                sb.AppendLine("        }");

                sb.AppendLine($"        public ICollection<T> GetContainer<T>()");
                sb.AppendLine("        {");
                sb.AppendLine("            return (ICollection<T>)Container.Values;");
                sb.AppendLine("        }");

                sb.AppendLine("    }");
                sb.AppendLine("");
            }

            // namespace end
            sb.AppendLine("}");

            // output file
            var targetPath = Path.Combine(settings.managerPath, _configManagerName + ".generated.cs");
            using (var sw = new StreamWriter(targetPath))
            {
                sw.Write(sb);
            }
        }

        public override void GenerateTest(string[] className, ExcelReaderSettings settings)
        {
            var sb = new StringBuilder();
            
            // using
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine("using NUnit.Framework;");
            sb.AppendLine("using Sorani.Yookoso.GameData;");
            sb.AppendLine("using UnityEditor;");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using System;");
            sb.AppendLine("");
            
            sb.AppendLine($"public class {_testFileName}"); 
            sb.AppendLine("{");
            sb.AppendLine("    private static void InitializeConfigManager()");
            sb.AppendLine("    {");
            foreach (var path in className)
            {
                if (path.Contains("~$")) continue;

                var name = Path.GetFileNameWithoutExtension(path);
                var dataPath = Path.Combine(settings.binPath, name + _binExt).Replace("\\", "/");

                sb.AppendLine(
                    $"        ConfigManager.Instance.LoadConfig(AssetDatabase.LoadAssetAtPath<TextAsset>(\"{dataPath}\"), typeof({name}), typeof({name}Container));");
            }
            sb.AppendLine("    }");
            sb.AppendLine("");
            
            // test code for every config file
            foreach (var path in className)
            {
                if (path.Contains("~$")) continue;
                var name = Path.GetFileNameWithoutExtension(path);
                
                sb.AppendLine("    [Test]");
                sb.AppendLine($"    public void {name.ToLower()}_data_test()");
                sb.AppendLine("    {");
                sb.AppendLine("        InitializeConfigManager();");
                sb.AppendLine($"        var configList = ConfigManager.Instance.GetContainer<{name}>();");
                sb.AppendLine("        bool failed = false;");
                sb.AppendLine("        foreach (var config in configList)");
                sb.AppendLine("        {");
                sb.AppendLine("            var type = config.GetType();");
                sb.AppendLine("            var methodInfo = type.GetMethod(\"Cast\");");
                sb.AppendLine("            if (methodInfo is null) break;");
                sb.AppendLine("            try");
                sb.AppendLine("            {");
                sb.AppendLine("                methodInfo.Invoke(config, null);");
                sb.AppendLine("            }");
                sb.AppendLine("            catch (Exception e)");
                sb.AppendLine("            {");
                sb.AppendLine("                failed = true;");
                sb.AppendLine("                Debug.LogError(e);");
                sb.AppendLine($"                Debug.LogError($\"[{name}] Config Error with ID: {{config.ID}}, Details: {{config}}\");");
                sb.AppendLine("            }");
                sb.AppendLine("        }");
                sb.AppendLine("        Assert.IsFalse(failed);");
                sb.AppendLine("    }");
                sb.AppendLine("");
            }
            
            sb.AppendLine("}");
            sb.AppendLine("");
            
            // output file
            var targetPath = Path.Combine(settings.testPath, _testFileName + ".generated.cs");
            using (var sw = new StreamWriter(targetPath))
            {
                sw.Write(sb);
            }
        }

        public override void GenerateData(string className, ExcelReaderSettings settings)
        {
            var classInfo = ReadExcelData(className + ".xlsx", settings);

            var containerType =
                Type.GetType("Sorani.Yookoso.GameData." + classInfo.className + "Container, Sorani.Yookoso.GameData",
                    true);
            var containerInstance = Activator.CreateInstance(containerType);

            var mapName = "Container";
            var mapField = containerInstance.GetType().GetProperty(mapName);
            var mapInstance = (IDictionary) Activator.CreateInstance(mapField.PropertyType);

            var idInfo = classInfo.properties[0];
            var idList = (IList) idInfo.value;

            for (var i = 0; i < idList.Count; ++i)
            {
                var classType =
                    Type.GetType("Sorani.Yookoso.GameData." + classInfo.className + ", Sorani.Yookoso.GameData");
                var configInstance = Activator.CreateInstance(classType);

                var configFields = configInstance.GetType().GetProperties();
                foreach (var f in configFields)
                {
                    var info = classInfo.properties.Find((t) => t.name == f.Name);
                    if (info == null) continue;
                    try
                    {
                        var valueList = (IList) info.value;
                        f.SetValue(configInstance,
                            valueList[i] is DBNull ? default : Convert.ChangeType(valueList[i], f.PropertyType));
                    }
                    catch
                    {
                        //DKLog.Log($"{className}:{f.Name}");
                        //DKLog.Log(e);
                    }
                }

                var idStr = idList[i].ToString();
                if (string.IsNullOrEmpty(idStr))
                {
                    continue;
                }

                var id = Int32.Parse(idStr);
                ((IDictionary) mapField.GetValue(containerInstance))[id] = configInstance;
            }

            using (var output = File.Create(Path.Combine(settings.binPath, className) + _binExt))
            {
                ((IMessage) containerInstance).WriteTo(output);
            }
        }

        private static string GetTypeProtoName(Type type)
        {
            if (type == typeof(String))
                return "string";

            if (type == typeof(Int32))
                return "int32";

            if (type == typeof(Int64))
                return "int64";

            if (type == typeof(Boolean))
                return "bool";

            if (type == typeof(Single))
                return "float";

            if (type == typeof(Double))
                return "double";

            return "string";
        }
    }
}
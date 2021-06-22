using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Text.RegularExpressions;
using ExcelDataReader;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
#endif
using UnityEditor;
using UnityEngine;

namespace DK.ExcelReader
{
    public class CodeGeneratorBase : ICodeGenerator
    {
        public virtual void GenerateBasicCode(string className, ExcelReaderSettings settings)
        {

        }

        public virtual void PostCodeGeneration(ExcelReaderSettings settings)
        {

        }

        public virtual void GenerateTest(string[] className, ExcelReaderSettings settings)
        {
            throw new NotImplementedException();
        }

        public virtual void GenerateData(string className, ExcelReaderSettings settings)
        {

        }

        public virtual void GenerateConfigManager(string[] classNames, ExcelReaderSettings settings)
        {

        }

        internal SerializableClassInfo ReadExcelData(string fileName, ExcelReaderSettings settings)
        {
            // generate scriptable object class name
            string className = Path.GetFileNameWithoutExtension(GetExcelFilePath(fileName, settings));

            // load excel file
            // FileStream file = File.Open(GetExcelFilePath(fileName, settings), FileMode.Open);
            FileStream file = new FileStream(GetExcelFilePath(fileName, settings), FileMode.Open,
                FileAccess.Read, FileShare.ReadWrite);

            IExcelDataReader excelData = ExcelReaderFactory.CreateOpenXmlReader(file);
            DataSet dataSet = excelData.AsDataSet();

            SerializableClassInfo serializableClassInfo = new SerializableClassInfo();

            // read first sheet only 
            using (var table = dataSet.Tables[0])
            {
                SerializableClassInfo sci = new SerializableClassInfo();
                sci.className = className;
                sci.properties = new List<PropertyInfo>();

                // read property names
                for (int i = 0; i < table.Columns.Count; ++i)
                {
                    PropertyInfo propertyInfo = new PropertyInfo();
                    propertyInfo.name = table.Rows[0][i].ToString();
                    propertyInfo.name = Regex.Replace(propertyInfo.name, "[ \r\n]", "");
                    if (propertyInfo.name == sci.className)
                        propertyInfo.name = "_" + propertyInfo.name;

                    propertyInfo.comment = table.Rows[1][i].ToString();
                    propertyInfo.comment = Regex.Replace(propertyInfo.comment, "(?<!\r)\n", "\r\n");

                    propertyInfo.type = GetTheType(table.Rows[2][i].ToString());

                    List<object> values = new List<object>();
                    for (int j = 3; j < table.Rows.Count; ++j)
                    {
                        values.Add(table.Rows[j][i]);
                    }

                    propertyInfo.value = values;

                    sci.properties.Add(propertyInfo);
                }
                sci.isTable = true;
                serializableClassInfo = sci;
            }

            file.Close();
            return serializableClassInfo;
        }

        internal Type GetTypeFromAssembly(Assembly generatedAssembly, string definedTypeName)
        {
            foreach (var definedType in generatedAssembly.DefinedTypes)
            {
                if (definedType.Name == definedTypeName)
                {
                    return definedType;
                }
            }

            Debug.LogErrorFormat("Cannot find type of name {0}", definedTypeName);
            return null;
        }

        internal Assembly CreateTypeInMemory(CodeCompileUnit codeUnit, ExcelReaderSettings settings)
        {
            Assembly compiledAssembly = null;

            CompilerParameters compilerParams = new CompilerParameters();
            compilerParams.ReferencedAssemblies.AddRange(new[]
            {
                // system
                @"System",

                // unity
                settings.unityEnginePath + @"UnityEngine",
                settings.unityEnginePath + @"UnityEngine.CoreModule",

                // odin
                @"Assets/Plugins/Sirenix/Assemblies/Sirenix.OdinInspector.Attributes",
                @"Assets/Plugins/Sirenix/Assemblies/Sirenix.OdinInspector.Editor",
                @"Assets/Plugins/Sirenix/Assemblies/Sirenix.Serialization"
            });

            compilerParams.GenerateInMemory = true;
            compilerParams.GenerateExecutable = false;

            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
            CompilerResults results = provider.CompileAssemblyFromDom(compilerParams, codeUnit);

            if (results.Errors != null && results.Errors.Count == 0)
            {
                compiledAssembly = results.CompiledAssembly;
            }
            else
            {
                foreach (var e in results.Errors)
                    Debug.LogError(e);
            }

            return compiledAssembly;
        }

        internal Type GetTheType(string type)
        {
            switch (type)
            {
                case "string":
                    return typeof(String);
                case "int":
                    return typeof(Int32);
                case "float":
                    return typeof(Single);
                case "long":
                    return typeof(Int64);
                case "double":
                    return typeof(Double);
                case "bool":
                    return typeof(Boolean);
                default:
                    return typeof(String);
            }
        }

        internal string GetExcelFilePath(string file, ExcelReaderSettings settings)
        {
            if (!Directory.Exists(settings.excelPaths))
            {
                Directory.CreateDirectory(settings.excelPaths);
            }

            return Path.Combine(settings.excelPaths, file);
        }

        public class SerializableClassInfo
        {
            public bool isTable = false;
            public string className;
            public List<PropertyInfo> properties = new List<PropertyInfo>();
        }

        public class PropertyInfo
        {
            public string comment;
            public string name;
            public Type type;
            public object value;
        }
    }
}

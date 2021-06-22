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

using ExcelDataReader;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
#endif

using UnityEditor;

using UnityEngine;

namespace DK.ExcelReader
{
    public class ScriptableObjectGenerator : CodeGeneraterBase
    {
        public override void GenerateBasicCode(string className, ExcelReaderSettings settings)
        {
            var classes = ReadExcelData(className + ".xlsx", settings);

            // create target unit;
            var targetUnit = new CodeCompileUnit();
            CodeNamespace iCns = new CodeNamespace("");

            // add imports
            iCns.Imports.Add(new CodeNamespaceImport("System"));
            iCns.Imports.Add(new CodeNamespaceImport("UnityEngine"));
            iCns.Imports.Add(new CodeNamespaceImport("Sirenix.OdinInspector"));

            // class imports
            CodeNamespace cns = new CodeNamespace("Sorani.Yookoso.GameData");
            targetUnit.Namespaces.Add(iCns);
            targetUnit.Namespaces.Add(cns);

            // scriptable object
            CodeTypeDeclaration ctd = new CodeTypeDeclaration(className);
            ctd.IsClass = true;
            ctd.TypeAttributes = TypeAttributes.Public;
#if ODIN_INSPECTOR
            ctd.BaseTypes.Add(typeof(SerializedScriptableObject));
#else
            ctd.BaseTypes.Add(typeof(UnityEngine.ScriptableObject));
#endif

            cns.Types.Add(ctd);

            // serializable objects
            CodeTypeDeclaration soCtd = new CodeTypeDeclaration(classes.className);
            soCtd.IsClass = true;
            soCtd.TypeAttributes = TypeAttributes.Public;
            soCtd.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference("System.Serializable")));

            // add properties
            foreach (var p in classes.properties)
            {
                CodeMemberField cmf = new CodeMemberField(p.type, p.name);

                CodeCommentStatement desc = new CodeCommentStatement();
                desc.Comment = new CodeComment(p.comment);

                cmf.Attributes = MemberAttributes.Public;
                cmf.Comments.Add(desc);

                soCtd.Members.Add(cmf);
            }

            cns.Types.Add(soCtd);

            // compile serializable classes into memory
            CodeCompileUnit tmpCCU = new CodeCompileUnit();
            tmpCCU.Namespaces.Add(cns);
            var assembly = CreateTypeInMemory(tmpCCU, settings);

            // add member to scriptable object
            //foreach (var soClass in classes)
            {
                Type type = null;
                Type soType = GetTypeFromAssembly(assembly, classes.className);

                if (!classes.isTable)
                {
                    type = soType;
                }
                else
                {
                    PropertyInfo idInfo =
                        classes.properties.Find(
                            target =>
                            target.name.Equals("id") ||
                            target.name.Equals("Id") ||
                            target.name.Equals("ID"));

                    // if there is a param named id, make it a dictionary

                    if (idInfo != null)
                    {
                        type = typeof(Dictionary<,>).MakeGenericType(idInfo.type, soType);
                    }
                    else
                    {
                        type = typeof(List<>).MakeGenericType(soType);
                    }
                }

                CodeMemberField cmf = new CodeMemberField(type, "_" + classes.className);
                cmf.Attributes = MemberAttributes.Public;
                ctd.Members.Add(cmf);
            }

            // compile to csharp
            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
            CodeGeneratorOptions options = new CodeGeneratorOptions();
            options.BracingStyle = "C";
            options.BlankLinesBetweenMembers = true;

            if (!Directory.Exists(settings.protoPath))
            {
                Directory.CreateDirectory(settings.protoPath);
            }

            // output file
            string targetPath = settings.protoPath + className + ".cs";
            using (StreamWriter sw = new StreamWriter(targetPath))
            {
                provider.GenerateCodeFromCompileUnit(targetUnit, sw, options);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public override void GenerateData(string className, ExcelReaderSettings settings)
        {
            // read excel data
            var classes = ReadExcelData(className + ".xlsx", settings);

            // create asset
            var asset = ScriptableObject.CreateInstance(className);

            var scriptableObjectFields = asset.GetType().GetFields();
            foreach (var field in scriptableObjectFields)
            {
                // create member instance
                var soObj = Activator.CreateInstance(field.FieldType);

                // load excel data for properties
                var classInfo = classes;
                if (classInfo != null)
                {
                    if (!classInfo.isTable)
                    {
                        var soObjFields = soObj.GetType().GetFields();
                        foreach (var f in soObjFields)
                        {
                            var info = classInfo.properties.Find((t) => { return t.name == f.Name; });
                            if (info != null)
                                f.SetValue(soObj, Convert.ChangeType(info.value, f.FieldType));
                            else
                                Debug.Log("fuck");
                        }
                    }
                    else
                    {
                        PropertyInfo idInfo =
                            classInfo.properties.Find(
                                target =>
                                target.name.Equals("id") ||
                                target.name.Equals("Id") ||
                                target.name.Equals("ID"));

                        if (idInfo != null)
                        {
                            // create Dictionary
                            var soObjDict = (IDictionary)soObj;

                            // get values 
                            var idList = (IList)idInfo.value;
                            for (int i = 0; i < idList.Count; ++i)
                            {
                                // create so object
                                Type classType = Type.GetType("Sorani.Yookoso.GameData." + classInfo.className + ", Assembly-CSharp");
                                var actualSoObject = Activator.CreateInstance(classType);
                                var asoFields = actualSoObject.GetType().GetFields();
                                foreach (var f in asoFields)
                                {
                                    var info = classInfo.properties.Find((t) => { return t.name == f.Name; });
                                    if (info != null)
                                    {
                                        try
                                        {
                                            var valueList = (IList)info.value;
                                            f.SetValue(actualSoObject, Convert.ChangeType(valueList[i], f.FieldType));
                                        }
                                        catch
                                        {
                                            Debug.Log(info.name);
                                        }
                                    }
                                    else
                                        Debug.Log("fuck");
                                }

                                // add actual value to dictionary 
                                soObjDict.Add(
                                    Convert.ChangeType(idList[i], soObjDict.GetType().GetGenericArguments()[0]),
                                    Convert.ChangeType(actualSoObject, soObjDict.GetType().GetGenericArguments()[1]));
                            }

                            soObj = soObjDict;
                        }
                        else
                        {
                            // create List
                            if (classInfo.properties.Count > 0)
                            {
                                var soObjList = (IList)soObj;
                                int index = 0;
                                PropertyInfo pInfo = classInfo.properties[0];
                                var firstValueList = (IList)pInfo.value;
                                while (index < firstValueList.Count)
                                {
                                    // create so object
                                    try
                                    {
                                        Type classType = Type.GetType("Sorani.Yookoso.GameData." + classInfo.className + ", Assembly-CSharp");
                                        var actualSoObject = Activator.CreateInstance(classType);
                                        var asoFields = actualSoObject.GetType().GetFields();
                                        foreach (var f in asoFields)
                                        {
                                            var info = classInfo.properties.Find((t) => { return t.name == f.Name; });
                                            if (info != null)
                                            {
                                                var valueList = (IList)info.value;
                                                f.SetValue(actualSoObject, Convert.ChangeType(valueList[index], f.FieldType));
                                            }
                                            else
                                                Debug.Log("fuck");
                                        }

                                        soObjList.Add(actualSoObject);
                                    }
                                    catch (System.Exception e)
                                    {
                                        Debug.Log(e);
                                    }

                                    index++;
                                }
                            }

                        }
                    }
                }
                else
                {
                    Debug.LogError("fuck");
                }

                // set member value
                field.SetValue(asset, soObj);
            }

            if (!Directory.Exists(settings.protoPath))
            {
                Directory.CreateDirectory(settings.protoPath);
            }

            AssetDatabase.CreateAsset(asset, settings.protoPath + className + ".asset");
            AssetDatabase.SaveAssets();
        }
    }
}

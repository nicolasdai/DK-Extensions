using UnityEditor;
using UnityEngine;
using System.IO;

namespace DK.ExcelReader
{
    public class ExcelReaderSettings : ScriptableObject
    {
        public string unityEnginePath = Path.GetDirectoryName(EditorApplication.applicationPath) + @"\Data\Managed\UnityEngine\";
        public string excelPaths;
        public string protoPath;
        public string csharpPath;
        public string managerPath;
        public string binPath;
        public string testPath;
    }
}
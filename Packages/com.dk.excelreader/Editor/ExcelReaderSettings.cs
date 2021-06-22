using UnityEngine;

namespace DK.ExcelReader
{
    public class ExcelReaderSettings : ScriptableObject
    {
        public string unityEnginePath = @"D:\2019.3.11f1\Editor\Data\Managed\UnityEngine\";
        public string excelPaths = @"";
        public string protoPath = @"";
        public string csharpPath = "";
        public string managerPath = @"";
        public string binPath;
        public string testPath;
    }
}
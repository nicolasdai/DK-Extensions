using UnityEngine;

namespace DK.Archive
{
    public class ArchiveConfig : ScriptableObject
    {
        public string keystorePassword;
        public string aliasPassword;

        public string hotfixDllName;
        public string hotfixDesPath;
    }
}


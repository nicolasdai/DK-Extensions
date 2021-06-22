using UnityEngine;

namespace DK.Archive
{
    public class VersionInfo : ScriptableObject
    {
        public int main = 1;
        public int patch;
        public int build;

        public string Version => $"{main}.{patch}.{build}";

        public void AddBuild()
        {
            build += Random.Range(50, 100);
        }

        public void AddPatch()
        {
            patch++;
        }

        public void AddMain()
        {
            main++;
        }
    }
}

using UnityEngine;

namespace DK.Archive
{
    public class VersionInfo : ScriptableObject
    {
        public int main = 1;
        public int patch = 0;
        public int build = 0;

        public string version
        {
            get
            {
                return string.Format("{0}.{1}.{2}", main, patch, build);
            }
        }

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

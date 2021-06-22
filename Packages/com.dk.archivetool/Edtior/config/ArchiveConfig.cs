using System.ComponentModel;
using UnityEngine;

namespace DK.Archive
{
    public class ArchiveConfig : ScriptableObject
    {
        [PasswordPropertyText]
        public string keystorePassword;
        
        [PasswordPropertyText]
        public string aliasPassword;
    }
}


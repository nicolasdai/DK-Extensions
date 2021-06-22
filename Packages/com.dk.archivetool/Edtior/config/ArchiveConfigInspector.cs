using System;
using System.Collections;
using System.Collections.Generic;
using DK.Archive;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ArchiveConfig))]
public class ArchiveConfigInspector : Editor
{
    private ArchiveConfig _archiveConfig;
    
    public void OnEnable()
    {
        _archiveConfig = target as ArchiveConfig;
    }

    public override void OnInspectorGUI()
    {
        _archiveConfig.keystorePassword = EditorGUILayout.PasswordField("Keystore Password:", _archiveConfig.keystorePassword);
        _archiveConfig.aliasPassword = EditorGUILayout.PasswordField("Alias Password:", _archiveConfig.aliasPassword);
    }
}

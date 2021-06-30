using DK.Archive;
using UnityEditor;

[CustomEditor(typeof(ArchiveConfig))]
public class ArchiveConfigInspector : Editor
{
    private SerializedProperty _keyStorePassword;
    private SerializedProperty _aliasPassword;
    private SerializedProperty _hotfixDllName;
    private SerializedProperty _hotfixDesPath;
    
    public void OnEnable()
    {
        _keyStorePassword = serializedObject.FindProperty("keystorePassword");
        _aliasPassword = serializedObject.FindProperty("aliasPassword");
        _hotfixDllName = serializedObject.FindProperty("hotfixDllName");
        _hotfixDesPath = serializedObject.FindProperty("hotfixDesPath");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        _keyStorePassword.stringValue = EditorGUILayout.PasswordField("Keystore Password", _keyStorePassword.stringValue);
        _aliasPassword.stringValue = EditorGUILayout.PasswordField("Alias Password", _aliasPassword.stringValue);

        _hotfixDllName.stringValue = EditorGUILayout.TextField("Hotfix Dll File Name", _hotfixDllName.stringValue);
        _hotfixDesPath.stringValue = EditorGUILayout.TextField("Hotfix Destination Path", _hotfixDesPath.stringValue);
        
        serializedObject.ApplyModifiedProperties();
    }
}

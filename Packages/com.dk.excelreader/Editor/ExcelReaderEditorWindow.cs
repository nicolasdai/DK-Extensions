using System.IO;
using UnityEditor;
using UnityEngine;

namespace DK.ExcelReader
{
    public class ExcelReaderEditorWindow : EditorWindow
    {
        private const string ToolSettings = @"Assets/DK-Extensions-Configs/excel_reader/";
        private const string PathSetting = @"path.asset";
        
        // private float smallBtnHeight = 30;
        private const float MediumBtnHeight = 50;
        // private float bigBtnHeight = 60;

        private ExcelReaderSettings _settings;
        private ExcelReader _excelReader;

        [MenuItem("DK/ExcelReader")]
        private static void OpenWindow()
        {
            var window = GetWindow<ExcelReaderEditorWindow>();
            window.position = new Rect(400, 400, 800, 600);
            
            window.Init();
            window.Show();
        }
        
        private void OnGUI()
        {
            if (_excelReader == null) Init();
            
            // display settings
            _settings.unityEnginePath = EditorGUILayout.TextField("Engine Path", _settings.unityEnginePath);
            _settings.excelPaths = EditorGUILayout.TextField("Excel Path", _settings.excelPaths);
            _settings.protoPath = EditorGUILayout.TextField("Proto Path", _settings.protoPath);
            _settings.csharpPath = EditorGUILayout.TextField("CSharp Path", _settings.csharpPath);
            _settings.managerPath = EditorGUILayout.TextField("Manager Path", _settings.managerPath);
            _settings.binPath = EditorGUILayout.TextField("Binary Path", _settings.binPath);
            _settings.testPath = EditorGUILayout.TextField("Test Path", _settings.testPath);
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate Code", GUILayout.Height(MediumBtnHeight))) _excelReader.GenerateAllConfigFile();
            if (GUILayout.Button("Generate Data", GUILayout.Height(MediumBtnHeight))) _excelReader.GenerateAllData();
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Basic Code Only", GUILayout.Height(MediumBtnHeight))) _excelReader.GenerateBasicCodeOnly();
            if (GUILayout.Button("Manager Only", GUILayout.Height(MediumBtnHeight))) _excelReader.GenerateConfigManagerOnly();
            if (GUILayout.Button("Test Code Only", GUILayout.Height(MediumBtnHeight))) _excelReader.GenerateTestCodeOnly();
            if (GUILayout.Button("Post Generation Only", GUILayout.Height(MediumBtnHeight))) _excelReader.PostGenerationOnly();
            GUILayout.EndHorizontal();
        }

        private void Init()
        {
            // load settings files
            if (!Directory.Exists(ToolSettings))
            {
                Directory.CreateDirectory(ToolSettings);
            }
            
            var pathSettingPath = Path.Combine(ToolSettings, PathSetting);
            if (!File.Exists(pathSettingPath))
            {
                var s = CreateInstance<ExcelReaderSettings>();
                AssetDatabase.CreateAsset(s, pathSettingPath);
                AssetDatabase.SaveAssets();
            }
            
            _settings = (ExcelReaderSettings)AssetDatabase.LoadAssetAtPath(pathSettingPath, typeof(ExcelReaderSettings));
            _excelReader = new ExcelReader(_settings);
        }
    }
}

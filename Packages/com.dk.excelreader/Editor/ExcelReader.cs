using System.IO;
using System.Linq;
using System.Collections.Generic;

using UnityEditor;

namespace DK.ExcelReader
{
    public class ExcelReader
    {
        private readonly ExcelReaderSettings _settings;
        private ICodeGenerator _generator;

        public ExcelReader(ExcelReaderSettings settings)
        {
            _settings = settings;
        }

        public void GenerateAllConfigFile()
        {
            _generator = new ProtoFileGenerator();
            
            // clear existed files
            var oldFiles = new List<string>();
            oldFiles.AddRange(Directory.GetFiles(_settings.protoPath));
            oldFiles.AddRange(Directory.GetFiles(_settings.csharpPath));

            foreach (var path in oldFiles.Where(File.Exists))
            {
                File.Delete(path);
            }

            var allExcels = Directory.GetFiles(_settings.excelPaths);
            foreach (var fullPath in allExcels)
            {
                // check extension
                if (Path.GetExtension(fullPath).Equals(".xlsx") &&
                    !Path.GetFileNameWithoutExtension(fullPath).Contains("~$"))
                {
                    _generator.GenerateBasicCode(Path.GetFileNameWithoutExtension(fullPath), _settings);
                }
            }

            _generator.GenerateConfigManager(allExcels, _settings);
            _generator.PostCodeGeneration(_settings);
        }

        public void GenerateAllData()
        {
            _generator = new ProtoFileGenerator();
            
            // clear existed files
            // clear existed files
            var oldFiles = new List<string>();
            oldFiles.AddRange(Directory.GetFiles(_settings.binPath));

            foreach (var path in oldFiles.Where(File.Exists))
            {
                File.Delete(path);
            }

            var allExcels = Directory.GetFiles(_settings.excelPaths);
            for (var index = 0; index < allExcels.Length; index++)
            {
                var fullPath = allExcels[index];
                // check extension
                var fileName = Path.GetFileNameWithoutExtension(fullPath);
                if (!Path.GetExtension(fullPath).Equals(".xlsx") || fileName.Contains("~$")) continue;

                EditorUtility.DisplayProgressBar("Generating Data", $"Handling {fileName}, {index} / {allExcels.Length}", (float)index / allExcels.Length);
                _generator.GenerateData(fileName, _settings);
            }
            
            EditorUtility.ClearProgressBar();
        }

        public void GenerateBasicCodeOnly()
        {
            _generator = new ProtoFileGenerator();

            var allExcels = Directory.GetFiles(_settings.excelPaths);
            foreach (var fullPath in allExcels)
            {
                // check extension
                if (Path.GetExtension(fullPath).Equals(".xlsx") &&
                    !Path.GetFileNameWithoutExtension(fullPath).Contains("~$"))
                {
                    _generator.GenerateBasicCode(Path.GetFileNameWithoutExtension(fullPath), _settings);
                }
            }
        }
        
        public void GenerateConfigManagerOnly()
        {
            _generator = new ProtoFileGenerator();

            var allExcels = Directory.GetFiles(_settings.excelPaths);
            _generator.GenerateConfigManager(allExcels, _settings);
        }
        
        public void GenerateTestCodeOnly()
        {
            _generator = new ProtoFileGenerator();

            var allExcels = Directory.GetFiles(_settings.excelPaths);
            _generator.GenerateTest(allExcels, _settings);
        }
        
        public void PostGenerationOnly()
        {
            _generator = new ProtoFileGenerator();

            _generator.PostCodeGeneration(_settings);
        }
    }
}

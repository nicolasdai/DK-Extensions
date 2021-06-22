namespace DK.ExcelReader
{
    public interface ICodeGenerator
    {
        void GenerateBasicCode(string className, ExcelReaderSettings settings);
        void PostCodeGeneration(ExcelReaderSettings settings);
        void GenerateConfigManager(string[] className, ExcelReaderSettings settings);
        void GenerateTest(string[] className, ExcelReaderSettings settings);
        void GenerateData(string className, ExcelReaderSettings settings);
    }
}

## Excel Reader

##### Description
This plug-in is used for auto generating csharp files and scriptable object assets from Excel.

##### Tutorial
1. Import plug-in，open window from Tools/DK/ExcelReader
2. Set required path in ToolSettings
    1. UnityEnginePath  is "Editor/Data/Managed/UnityEngine/" under Unity Editor install path，this path is to locate UnityEngine.dll，it will be used for temporary compile duration csharp generation。
    2. ExcelPaths is for excel file path, it can be inside the unity project or not.
    3. Output Path is the path for auto generated files

3. In Data Generator，click Generate CSharp first，csharp file will be generated and Unity will refresh automatically and compile.
4. After compile is finished, click Generate Data，generate related asset files.
5. Notice，seperate this into two buttons to wait for compilaiton, dont modify excel files during these two processes；
7. Everytime the header of excel is changed, regenerate csharp file；if only data has been changed in excel, you can regenerate data only.

##### Excel file sturcture rules
1. Every excel file will generate one Scriptable Object class, every sheet in this excel file will genertate a serializable class. ScriptableObject will contain a variable for each Serializable class. The type of the variable will be the type of serializable class or a Dictionary/List.
2. For non-table sheet, sheet name will be the class name; in Excel file, column 1 is description, it be comment in csharp; column 2 is variable name; column 3 is variable type; column 4 is variable value.
3. For table sheet, sheet name should add @table as postfix, eg: someType@table; in Excel file, row 1 is description, it will be comment in csharp; row 2 is variable name; row 3 is variable type; from row 4 and above are variable values; this structure is just like database.
4. When there is a variable named ID/Id/id, the variable type in Scriptable Object for this sheet will be Dictionary, otherwise it will be List.
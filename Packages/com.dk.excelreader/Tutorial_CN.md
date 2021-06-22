## Excel Reader

##### 说明
本插件可以直接将Excel内的数据按照Excel表结构，自动生成csharp代码，并生成ScriptableObject数据资源

##### 使用方法
1. 导入插件后，在Tools/DK/ExcelReader打开窗口
2. 在ToolSettings中设置相关的路径
    1. UnityEnginePath为当前使用的Unity的安装路径下的Editor/Data/Managed/UnityEngine/，此路径的目的是定位UnityEngine的dll文件，在生成csharp代码的过程中用于临时编译。
    2. ExcelPaths为Excel文件所在的路径，可以在Unity项目中也可以不在。
    3. Output Path为输出文件路径，在此路径中会分别生成csharp文件和对应的asset文件

3. 在Data Generator中，先点击Generate CSharp按钮，点击之后会自动生成csharp文件，Unity会自动刷新，编译新生成的代码
4. 编译完成之后点击Generate Data按钮，生成对应的asset文件
5. 注意，两个步骤分成两个按钮是因为需要等待Unity编译
6. 在操作过程中不可以修改Excel文件；
7. 每次Excel文件的表结构被修改之后，需要重新生成csharp文件；Excel只有数据被修改，可以直接重新生成Data

##### Excel文件结构规则
1. 每个Excel'文件对应一个ScriptableObject类，Excel中的每个Sheet对应一个Serializable类。ScriptableObject中包含所有Serializable类的变量，变量类型为类型本身或者Dictionary/List类型。
2. 非表单类的Sheet，即每个配置只有一个值；Sheet名为类型名；Excel第一列为描述，会作为注释出现在csharp代码中；Excel第二列为变量名；第三列为变量类型；第四列为值。
3. 表单类的Sheet，即配置可以有多个值；Sheet名后需要添加@table关键字，例如someTable@table；Excel第一行为描述，会作为注释出现在csharp代码中；第二行为变量名；第三行为变量类型；从第四行开始到最后一行，为所有的值。此结构类似数据库结构。
4. 当表单类的sheet中包含有变量名ID/Id/id时，对应的成员将被声明为Dictionary类型；否则将会被声明为List类型。
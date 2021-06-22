set exePath=%1 
::set exePath="E:/_Sorani_Devspace/DK-Extensions/Packages/com.dk.excelreader/tools/protoc.exe"
set outputPath=%2
::set outputPath="E:/_Sorani_Devspace/DK-Extensions/Assets/_Scripts/3-GameData/DataDefine/AutoGenCSharp"
set protoPath=%3
::set protoPath="E:/_Sorani_Devspace/DK-Extensions/Assets/_Scripts/3-GameData/DataDefine/Proto"

%exePath% --proto_path=%protoPath% --csharp_out=%outputPath% %protoPath%/*.proto
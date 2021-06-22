set exePath=%1 
::"E:/_Sorani_Devspace/Pixel_Coffee_Office/Assets/DK-Extensions/excel_reader/tools/protoc"
set outputPath=%2
::"E:/_Sorani_Devspace/Pixel_Coffee_Office/Assets/_Scripts/3-GameData/DataDefine/Generated"
set protoPath=%3
::"E:/_Sorani_Devspace/Pixel_Coffee_Office/Assets/_Scripts/3-GameData/DataDefine/AutoGen/"

%exePath% --proto_path=%protoPath% --csharp_out=%outputPath% %protoPath%/*.proto
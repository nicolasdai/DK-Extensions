#!/bin/bash
export exePath=$1
export outputPath=$2
export protoPath=$3

$exePath --proto_path=$protoPath --csharp_out=$outputPath $protoPath/*.proto

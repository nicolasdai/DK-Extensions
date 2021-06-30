#!/bin/bash
export outputPath=$1
export protoPath=$2

protoc --proto_path=$protoPath --csharp_out=$outputPath $protoPath/*.proto
protoc --proto_path=$protoPath --go_out=/Users/mac/StaticData $protoPath/*.proto
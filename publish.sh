#!/usr/bin/env bash
time dotnet publish --output bin/Release/linux-x64 --configuration Release --runtime linux-x64 --self-contained --verbosity minimal /p:PublishTrimmed=true

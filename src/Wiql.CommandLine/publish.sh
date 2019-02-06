#!/bin/bash

dotnet build -c Release -r osx.10.13-x64
dotnet publish -c Release -r osx.10.13-x64  --self-contained true

dotnet build -c Release -r win10-x64
dotnet publish -c Release -r win10-x64  --self-contained true

dotnet build -c Release -r linux-x64
dotnet publish -c Release -r linux-x64  --self-contained true
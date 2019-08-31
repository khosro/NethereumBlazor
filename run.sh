#!/bin/bash

dotnet run --no-launch-profile --no-build -c Release -p "NethereumBlazor/NethereumBlazor.csproj" -- $@

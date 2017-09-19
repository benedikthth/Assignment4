#!/bin/bash
cd API/
dotnet restore
cd ..
cd Models
dotnet restore
cd ..
cd Services
dotnet restore
cd ..
cd Tests
dotnet restore
echo "Done"

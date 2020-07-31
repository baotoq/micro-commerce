#!/bin/bash

# exit on error
set -e
# debug trace
set -o xtrace

cd ./src
cd ./Catalog.API
dotnet ef database drop -f --context ApplicationDbContext
dotnet ef database update --context ApplicationDbContext

cd ..
cd ./Identity.API
dotnet ef database drop -f --context ApplicationDbContext
dotnet ef database update --context ApplicationDbContext
dotnet ef database update --context ConfigurationDbContext
dotnet ef database update --context PersistedGrantDbContext

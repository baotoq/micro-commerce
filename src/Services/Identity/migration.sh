#!/bin/bash

# exit on error
set -e
# debug trace
set -o xtrace

dotnet build MicroCommerce.Identity.Web

for i in IdentityServerConfigurationDbContext IdentityServerPersistedGrantDbContext AdminIdentityDbContext IdentityServerDataProtectionDbContext IdentityServerConfigurationDbContext
do
    dotnet ef migrations script -o $i.sql -s MicroCommerce.Identity.Web -p Shared/MicroCommerce.Identity.EntityFramework.Npgsql -c $i --no-build
done


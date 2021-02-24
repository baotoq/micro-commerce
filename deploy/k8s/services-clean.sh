#!/bin/bash

# exit on error
set -e
# debug trace
set -o xtrace

for i in react-web catalog-api identity-api
do
    helm uninstall $i -n app
done
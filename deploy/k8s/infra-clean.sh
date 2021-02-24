#!/bin/bash

# exit on error
set -e
# debug trace
set -o xtrace

for i in postgres
do
    helm uninstall $i -n infra
done
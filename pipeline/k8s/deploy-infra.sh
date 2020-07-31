#!/bin/bash

# exit on error
set -e
# debug trace
set -o xtrace

kubectl create namespace infrastructure || true

cd infrastructure

cd nginx-ingress
helm upgrade nginx-ingress stable/nginx-ingress --install -f ./values.yaml --namespace=default
cd ..

cd mssql-linux
helm upgrade mssql stable/mssql-linux --install -f ./values.yaml --namespace=infrastructure
cd ..

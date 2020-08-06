#!/bin/bash

# exit on error
set -e
# debug trace
set -o xtrace

kubectl create namespace infra || true

cd infra

cd nginx-ingress
helm upgrade nginx-ingress stable/nginx-ingress --install -f ./values.yaml --namespace=default
cd ..

cd postgres
helm upgrade postgres cetic/postgresql --install -f ./values.yaml --namespace=infra
cd ..

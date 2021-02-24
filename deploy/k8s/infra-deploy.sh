#!/bin/bash

# exit on error
set -e
# debug trace
set -o xtrace

kubectl create namespace infra || true
kubectl label namespace infra istio-injection=enabled || true

cd infra

cd postgres
helm upgrade postgres bitnami/postgresql --install -f ./values.yaml --namespace=infra
cd ..

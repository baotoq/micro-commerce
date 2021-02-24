#!/bin/bash

# exit on error
set -e
# debug trace
set -o xtrace

cd infra

cd istio
kubectl apply -f gateway.yml --namespace=default
cd ..
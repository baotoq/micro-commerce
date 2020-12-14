#!/bin/bash

# exit on error
set -e
# debug trace
set -o xtrace

kubectl create namespace infra || true
kubectl create namespace jenkins || true
kubectl label namespace infra istio-injection=enabled || true

cd infra

cd istio
kubectl apply -f gateway.yml --namespace=default
cd ..

cd postgres
helm upgrade postgres cetic/postgresql --install -f ./values.yaml --namespace=infra
cd ..

cd jenkins
helm upgrade jenkins stable/jenkins --install -f ./values.yaml --namespace=jenkins
cd ..
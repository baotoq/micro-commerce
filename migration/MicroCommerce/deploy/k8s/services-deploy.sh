#!/bin/bash

# exit on error
set -e
# debug trace
set -o xtrace

kubectl create namespace bshop || true
kubectl label namespace bshop istio-injection=enabled || true

for i in react-web catalog-api identity-api
do
    cd $i
    helm package .
    helm lint -f ./values.yaml -f values.local.yml .
    helm upgrade $i --install -f ./values.yaml -f values.local.yml --namespace=bshop .
    cd ..
done

#!/bin/bash

# exit on error
set -e
# debug trace
set -o xtrace

kubectl create namespace bshop || true
kubectl create namespace infrastructure || true

for i in react-web catalog-api identity-api
do
    cd $i
    helm package .
    helm lint -f ./values.yaml -f values.local.yml .
    helm upgrade $i --install -f ./values.yaml -f values.local.yml --namespace=bshop .
    cd ..
done

cd infrastructure

cd nginx-ingress
helm upgrade nginx-ingress stable/nginx-ingress --install -f ./values.yaml --namespace=default
cd ..

cd mssql-linux
helm upgrade mssql stable/mssql-linux --install -f ./values.yaml --namespace=infrastructure
cd ..

cd sonarqube
helm upgrade sonar oteemocharts/sonarqube --install -f ./values.yaml -f values.local.yml --namespace=infrastructure
cd ..
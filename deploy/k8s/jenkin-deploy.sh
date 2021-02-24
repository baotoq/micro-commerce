#!/bin/bash

# exit on error
set -e
# debug trace
set -o xtrace

kubectl create namespace jenkins || true

cd infra

cd jenkins
helm upgrade jenkins stable/jenkins --install -f ./values.yaml --namespace=jenkins
cd ..
#!/bin/bash

# exit on error
set -e
# debug trace
set -o xtrace

helm repo add bitnami https://charts.bitnami.com/bitnami

helm repo update

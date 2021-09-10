#!/bin/bash

# exit on error
set -e
# debug trace
set -o xtrace

mkcert -install
copy $env:LOCALAPPDATA\mkcert\rootCA.pem ./cacerts.pem
copy $env:LOCALAPPDATA\mkcert\rootCA.pem ./cacerts.crt

mkcert -cert-file microcommerce.localhost.crt -key-file microcommerce.localhost.key microcommerce.localhost *.microcommerce.localhost
mkcert -pkcs12 microcommerce.localhost.pfx microcommerce.localhost *.microcommerce.localhost

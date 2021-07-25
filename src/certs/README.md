# Run

``` shell
mkcert -install
copy $env:LOCALAPPDATA\mkcert\rootCA.pem ./cacerts.pem
copy $env:LOCALAPPDATA\mkcert\rootCA.pem ./cacerts.crt
```

``` shell
mkcert -cert-file microcommerce.localhost.crt -key-file microcommerce.localhost.key microcommerce.localhost *.microcommerce.localhost
mkcert -pkcs12 microcommerce.localhost.pfx microcommerce.localhost *.microcommerce.localhost
```

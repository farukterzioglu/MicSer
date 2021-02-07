### Static host list 
`docker run --name krakend --rm -p 8443:18443 -v $PWD/static:/etc/krakend/ devopsfaith/krakend`

### Service discovery with etcd
`docker run --name krakend --rm -p 8443:18443 -v $PWD/etcd:/etc/krakend/ devopsfaith/krakend`

### Service discovery with Consul
`docker run --name krakend --rm -p 8443:18443 -v $PWD/consul:/etc/krakend/ devopsfaith/krakend`
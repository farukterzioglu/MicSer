docker exec fox /bin/sh -c "echo '{\"service\": {\"name\": \"bitcoind\", \"tags\": [\"bitcoin\"], \"port\": 18443}}' >> /consul/config/bitcoind.json"
docker exec fox /bin/sh -c "echo '{\"service\": {\"name\": \"bitcoind\", \"tags\": [\"bitcoin\"], \"address\": \"host.docker.internal\", \"port\": 18443}}' >> /consul/config/bitcoind.json"

curl --request POST 'http://127.0.0.1:18443' --header 'Authorization: Basic bXl1c2VyOlZzeEdsY3NZSkkyRFd2ZTVURjV6cXIzaElHWDFodHhkT0hPOGQxeUEyalk9' --data-raw '{\"jsonrpc\":\"1.0\",\"id\":\"curltext\",\"method\":\"getblockchaininfo\",\"params\":[]}'


curl --location --request POST 'http://127.0.0.1:19443' --header 'Authorization: Basic bXl1c2VyOlZzeEdsY3NZSkkyRFd2ZTVURjV6cXIzaElHWDFodHhkT0hPOGQxeUEyalk9' --header 'Content-Type: application/json' --data-raw '{"jsonrpc":"1.0","id":"curltext","method":"getblockchaininfo","params":[]}'


## Create a Local Consul Datacenter
https://learn.hashicorp.com/tutorials/consul/get-started-create-datacenter?in=consul/getting-started
// @ Host  
consul agent  -ui -node=agent-three   -bind=172.20.20.1  -enable-script-checks=true   -data-dir=/tmp/consul   -config-dir=/etc/consul.d

## Vagrant | Configure port forwarding
https://learn.hashicorp.com/tutorials/vagrant/getting-started-networking#configure-port-forwarding

## vm1
consul agent  -server   -bootstrap-expect=1   -node=agent-one   -bind=172.20.20.10   -data-dir=/tmp/consul   -config-dir=/etc/consul.d -enable-script-checks=true
consul join 172.20.20.11
consul join 172.20.20.1
consul members
// consul reload
docker run --rm --name bitcoind-0.20.1     --volume $HOME/.bitcoin-regtest-0.20.1:/root/.bitcoin     -p 0.0.0.0:18443:18443     farukter/bitcoind:regtest-0.20.1

## vm2
consul agent   -node=agent-two   -bind=172.20.20.11   -enable-script-checks=true   -data-dir=/tmp/consul   -config-dir=/etc/consul.d
sudo docker run --rm --name bitcoind-0.21.0     --volume $HOME/.bitcoin-regtest-0.21.0:/root/.bitcoin     -p 0.0.0.0:18443:18443     farukter/bitcoind:regtest-0.21.0

## vm1
nano /etc/consul.d/bitcoind.json
echo '{
  "service": {
    "name": "bitcoind",
    "tags": [
      "live",
      "0.20.1"
    ],
    "port": 18443,
    "check": {
      "args": [
        "docker", "exec", "bitcoind-0.20.1", "bitcoin-cli", "getblockchaininfo"
      ],
      "interval": "10s"
    }
  }
}' > bitcoind.json

## vm2
nano /etc/consul.d/bitcoind.json
echo '{
  "service": {
    "name": "bitcoind",
    "tags": [
      "live",
      "0.21.0"
    ],
    "port": 18443,
    "check": {
      "args": [
        "docker", "exec", "bitcoind-0.21.0", "bitcoin-cli", "getblockchaininfo"
      ],
      "interval": "10s"
    }
  }
}' > bitcoind.json

// host 
nano /etc/resolver/consul

nameserver 127.0.0.1
port 8600

docker run --rm --name bitcoind-0.21.0 --volume $HOME/.bitcoin-regtest-0.21.0:/root/.bitcoin -p 127.0.0.1:18443:18443 farukter/bitcoind:regtest-0.21.0

curl --location --request POST 'http://127.0.0.1:18443' --header 'Authorization: Basic bXl1c2VyOlZzeEdsY3NZSkkyRFd2ZTVURjV6cXIzaElHWDFodHhkT0hPOGQxeUEyalk9' --header 'Content-Type: application/json' --data-raw '{"jsonrpc":"1.0","id":"curltext","method":"generatetoaddress","params":[1,"2MwsHLRMeBgkvb5cwwSMNC3qVDXpt7hX7qw"]}'

docker run --rm --name bitcoind-0.20.1 --volume $HOME/.bitcoin-regtest-0.20.1:/root/.bitcoin -p 127.0.0.1:18444:18443 farukter/bitcoind:regtest-0.20.1

curl --location --request POST 'http://127.0.0.1:18444' --header 'Authorization: Basic bXl1c2VyOlZzeEdsY3NZSkkyRFd2ZTVURjV6cXIzaElHWDFodHhkT0hPOGQxeUEyalk9' --header 'Content-Type: application/json' --data-raw '{"jsonrpc":"1.0","id":"curltext","method":"generatetoaddress","params":[1,"2MwsHLRMeBgkvb5cwwSMNC3qVDXpt7hX7qw"]}'

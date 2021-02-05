docker run --rm --name bitcoind-0.20.1 \
    --volume $HOME/.bitcoin-regtest-0.20.1:/root/.bitcoin \
    -p 127.0.0.1:18444:18443 \
    farukter/bitcoind:regtest-0.20.1

docker run --rm --name bitcoind-0.21.0 \
    --volume $HOME/.bitcoin-regtest-0.21.0:/root/.bitcoin \
    -p 127.0.0.1:18443:18443 \
    farukter/bitcoind:regtest-0.21.0
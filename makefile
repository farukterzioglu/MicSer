PHONY : build-secured-api build-identity-server run-identity-server

DOCKERBUILD=docker build 
DOCKERSTOP=docker stop
DOCKERRUN=docker run
HOSTIP="172.19.0.2:9092"

build-secure: build-identity-server build-secure-api
run-secure: run-identity-server run-secure-api

build-identity-server: 
	@echo "\033[36mBuilding identity server...\033[0m"
	$(DOCKERBUILD) -f ./src/MicSer.IdentityServer/Dockerfile -t identityserver:latest ./src/MicSer.IdentityServer/.

run-identity-server:
	@echo "\033[36mRunning identity server api...\033[0m"
	$(DOCKERSTOP) identityserver || true
	$(DOCKERRUN) --rm -d -e ASPNETCORE_ENVIRONMENT=Development -p 8081:80 --name identityserver identityserver:latest

build-secure-api: 
	@echo "\033[36mBuilding secured api...\033[0m"
	$(DOCKERBUILD) -f ./src/MicSer.SecuredApi/Dockerfile -t securedapi:latest ./src/MicSer.SecuredApi/.

run-secure-api:
	@echo "\033[36mRunning secured api...\033[0m"
	$(DOCKERSTOP) securedapi && $(DOCKERSTOP) secured-api-gw || true
	$(DOCKERRUN) --rm -d -e ASPNETCORE_ENVIRONMENT=Development -p 6001:80  --name securedapi securedapi:latest
	$(DOCKERRUN) --rm -d -p 8080:8080 -v $$PWD/src/MicSer.SecuredApi:/etc/krakend/ --name secured-api-gw devopsfaith/krakend
curl --location --request GET 'http://localhost:6001/WeatherForecast' --header 'Authorization: Bearer [TOKEN]'

docker run --rm -p 8080:8080 -v $PWD:/etc/krakend/ devopsfaith/krakend  
curl --location --request GET 'http://localhost:8080/weatherforecast'  


// Problem with issuer name   
https://github.com/IdentityServer/IdentityServer4/issues/501#issuecomment-377936562  

docker build -t securedapi .  
docker run -it -e ASPNETCORE_ENVIRONMENT=Development -p 6001:80 securedapi:latest  
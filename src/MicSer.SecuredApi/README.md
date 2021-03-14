curl --location --request GET 'http://localhost:6001/WeatherForecast' --header 'Authorization: Bearer [TOKEN]'

docker run --rm -p 8080:8080 -v $PWD:/etc/krakend/ devopsfaith/krakend
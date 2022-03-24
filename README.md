# otel-tracing-api-worker

This repo contains : 
* WebApi project wich acts as a Rest API
* Worker Service wich acts as a background process to asynchronously persists data to a database.

## How to run this project

You can lauch this project from VisualStudio 2022 : it's already prepared to run both : webapi and worker service. But this could be run from VSCode as well.

Before running the projects you must provision a RabbitMQ and a Zipkin instance. In order to keep things simple, you can use docker, like this : 

#### RabbitMQ
docker run -d -p15672:15672 -p 5672:5672 rabbitmq:management

#### Zipkin
docker run -d -p 9411:9411 openzipkin/zipkin

Then you can run the solution from the IDE : VSCode or Visual Studio 2022 (gots to be 2022 since the project is NET6)

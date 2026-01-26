# RabbitMQ.MessageAPI

A lightweight .NET Web API project demonstrating event-driven communication using RabbitMQ.

This project showcases a simple **producer–consumer** architecture where messages are published via an HTTP API and processed asynchronously by a background worker.

## Purpose
The goal of this project is to demonstrate:
- Basic RabbitMQ messaging concepts
- Event-driven communication in .NET
- Separation of concerns between message producers and consumers

## Architecture Overview
- **Producer**: ASP.NET Core Web API endpoint publishes messages to RabbitMQ
- **Broker**: RabbitMQ (Direct Exchange)
- **Consumer**: BackgroundService that consumes and processes messages

## Tech Stack
- .NET 10
- ASP.NET Core Web API
- RabbitMQ
- RabbitMQ.Client
- OpenAPI / Swagger

## RabbitMQ Setup
- Exchange: `ex.messages` (Direct, Durable)
- Queue: `q.messages` (Durable)
- Routing Key: `messages.create`
- Message acknowledgment: Manual ACK
- Prefetch count: `1`

## How It Works
1. A client sends a request to the API endpoint
2. The API publishes a message to RabbitMQ
3. The consumer listens to the queue
4. Upon successful processing, the message is acknowledged

## Running the Project

### Start RabbitMQ (Docker)
```bash
docker run -d --name rabbitmq \
  -p 5672:5672 -p 15672:15672 \
  -e RABBITMQ_DEFAULT_USER=admin \
  -e RABBITMQ_DEFAULT_PASS=admin \
  rabbitmq:3-management

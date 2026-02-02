**MessageAPI**

MessageAPI is an event-driven backend system built with .NET 10 and designed according to Clean Architecture principles.
The project aims to provide a clean, modular, and extensible foundation for handling business workflows through asynchronous messaging and conversational user interactions.

The system integrates:

RabbitMQ for asynchronous communication and background processing

Telegram Bot as a lightweight, real-time user interaction layer

A future-ready authentication and authorization model, implemented in the Application layer

The architecture is intentionally designed to separate business rules from infrastructure and delivery mechanisms, ensuring long-term maintainability and scalability as the system grows.

**Project Goals**

The main goals of this project are:

Correct application of Clean Architecture
Business logic is isolated from external frameworks, databases, message brokers, and UI technologies. Dependencies always point inward, and each layer has a clearly defined responsibility.

Asynchronous, event-driven workflow management
RabbitMQ is used to publish and consume business messages, enabling long-running or resource-intensive operations to be processed outside the HTTP request lifecycle.

Conversational user interaction via Telegram
Telegram acts as an external interaction channel where users can trigger actions, receive system feedback, and interact with workflows without a traditional web interface.

Centralized business logic in the Application layer
All core decisions and workflows are handled in the Application layer, including:

user authentication and identity validation

authorization and permission checks

business rules and use-case orchestration

coordination between RabbitMQ producers and consumers

API controllers, Telegram bot handlers, and message consumers are kept thin and do not contain business logic; they only act as entry points that delegate work to the Application layer.

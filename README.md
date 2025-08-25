# AICalendar API

AICalendar API is a .NET 9 ASP.NET Core Web API designed for comprehensive personal calendar management. It supports multiple users, robust event handling, and a unique feature to find the nearest available time slots for all participants.

## Features

- **User Management:**

  - Register new users.
  - Login and obtain a JWT token for authentication.
  - Authorized users can update and delete their own profiles.
  - Retrieve all events an authenticated user is participating in.
  - Retrieve events created by the authenticated user.
  - Get a list of all registered users.

- **Event Management:**

  - Retrieve an event by its unique ID.
  - Retrieve all events with optional filtering.
  - Create new events.
  - Delete events (creator only).
  - Update events (creator only).
  - Cancel events (creator only).

- **Event Participants:**

  - Authenticated users can retrieve all participants for events they have created.
  - Add participants to an event.
  - Remove participants from an event.

- **Availability Finding:**

  - Find one or more nearest available time slots for all participants based on their availability.

## API Endpoints

The API endpoints are documented and accessible via Swagger UI.

**Swagger UI URL:** `https://localhost:<port>/swagger/index.html`

Below is a snapshot of the available API endpoints from Swagger UI:

![Swagger UI Endpoints](![alt text](images/image-1.png))

## .NET Aspire Orchestration

This project utilizes .NET Aspire for orchestration and local development. The Aspire dashboard provides a centralized view of all running resources.

![.NET Aspire Dashboard](![alt text](images/dashboard.png))

## Model Context Protocol (MCP) Implementation

The AICalendar API implements a Model Context Protocol (MCP) server that provides a flexible and efficient way to interact with the calendar system's core functionality.

### What is MCP?

Model Context Protocol (MCP) is a communication protocol that enables seamless interaction between clients and servers through well-defined tools and contexts. It provides a structured way to expose server-side functionality as tools that clients like Copilot, Claude or custom one can discover and invoke.

### MCP Server

The AICalendar MCP server (`AiCalendar.MCPServer`) exposes several tool sets for managing calendar operations:

- **Event Participants Tools:**
  - Get event participants
  - Add participants to events
  - Remove participants from events

- **User Management Tools:**
  - User registration
  - User authentication
  - User listing and filtering
  - User deletion

- **Event Management Tools:**
  - Create events
  - Retrieve events by ID
  - Retrieve all events with filtering options
  - Update and delete events (creator only)
  - Cancel events (creator only)

### MCP Endpoint

The MCP server is accessible at the `/mcp` endpoint and supports HTTP transport. The server implements rate limiting and CORS policies for security and resource management.

### Using MCP Clients

Clients can connect to the MCP server using any MCP-compatible client implementation. The server provides tool discovery capabilities, allowing clients to:

1. Discover available tools and their descriptions
2. Execute tools with proper parameters
3. Handle responses in a structured format (JSON)

Some MCP operations require proper authentication using JWT tokens for secure access to the system's functionality.

### Example MCP Server Usage

#### MCP Inspector

![MCP Inspector](![alt text](images/mcp-inspector.png))

#### Postman

![Postman](![alt text](images/postman.png))

#### Github Copilot Chat

![Github Copilot Chat](![alt text](images/copilot.png))

## Monitoring
As you know one of the main objectives of .NET Aspire is to provide a centralized view of all running resources. 
The AICalendar leverages this feature to monitor the health and performance of the application.
.NET Aspire integrations automatically set up Logging, Tracing, and Metrics configurations, which are sometimes known as the pillars of observability, using the .NET OpenTelemetry SDK.
You can view the logs, traces, and metrics in the .NET Aspire dashboard. But you can also export the collected metrics
from OpenTelemetry to other systems like Azure Monitor, Prometheus, or Grafana for further analysis and visualization.
In the AiCalendar.ServiceDefault you can find the `OpenTelemetryConfiguration` class that sets up the OpenTelemetry SDK for logging, tracing, and metrics.
There you can also add configuration which exports the collected metrics to /metrics endpoint. Then you can configure prometheus to scrape the metrics from this endpoint. After that you
can also visualize the metrics in Grafana and setup Alertmanager.

I run the Prometheus, Grafana, and Alertmanager in a Docker containers using the .NET Aspire API calls which provide you the option to orchestrate
your projects, docker containers and other dependencies (Redis, Kafka, etc.) with C# code instead of docker-compose file with YAML syntax. 

I also configured Node Exporter which is a Prometheus exporter for hardware and OS metrics exposed by *nix kernels, which can be used to monitor the host machine.
After some problems with running the Node Exporter i i found out that the problem and solved it. The solution was very simple.
I decided to import second dashboard for the Node Exporter metrics from the [Grafana Dashboard](https://grafana.com/grafana/dashboards/1860-node-exporter-full/).

![Node Exporter Dashboard](![alt text](images/node_exporter.png))

### Grafana Dashboard
I downloaded the Grafana dashboard from the [Grafana Dashboard](https://grafana.com/grafana/dashboards/19924-asp-net-core/) and imported it into my Grafana instance.

![Grafana Dashboard](![alt text](images/grafana.png))

### Alertmanager
I configured the Alertmanager to send alerts to Dicord channel. I created a Discord webhook and configured the Alertmanager to use it.

![Alertmanager](![alt text](images/discord.png))

## Deployment
I tried to deploy the app to Azure, but i had some problems with the deployment.
I am using student Azure account from SoftUni Software University which has some limitations.
Before trying to deploy the app to Azure, I red the Microsoft documentation about how to deploy .net Aspire app in azure.
They have a very good documentation about it. In the documetation they had shown two ways to do it.
	1. Using the Azure CLI
	2. Using Visual Studio
I tried to deploy the app using Azure CLI. I tried it for two days in each region. Some gave an error right from the start, while others did so after a few minutes.
I started wondering what was going on. Was I doing something wrong? But I kept trying. 
After many unsuccessful attempts, I remembered that when we were studying Azure at SoftUni Software University, I had similar problems when completing my tasks. 
There were times when the applications could not be deployed. Or they were deployed with great difficulty. It's just that our student accounts are very limited.
But despite the unsuccessful deployment, I am glad that I tried and now have an idea of how to deploy .NET Aspire app. 


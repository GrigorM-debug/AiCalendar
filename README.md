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

![Swagger UI Endpoints](![alt text](image-1.png))

## .NET Aspire Orchestration

This project utilizes .NET Aspire for orchestration and local development. The Aspire dashboard provides a centralized view of all running resources.

![.NET Aspire Dashboard](![alt text](image.png))

## TODOs

- Fix some things
- Unit Tests
- Mcp server

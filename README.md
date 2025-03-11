# Banking Web Application

This repository contains the source code for a Banking Web Application, built using .NET 8.0, MongoDB, and Docker.

## Prerequisites

Before running the application, ensure you have the following installed:

* Docker
* Docker Compose

## Getting Started

1.  **Clone the Repository:**

    ```bash
    git clone https://github.com/bohdantiranu/BankingWebappTestTask.git
    cd BankingWebApp
    ```

2.  **Run the Application:**

    Use Docker Compose to build and run the application:

    ```bash
    docker compose up --build -d
    ```

    This command will build the API container and start all services defined in `docker-compose.yml` in detached mode.

3.  **Initialize MongoDB Replica Set (First Time Only):**

    If you are running the application for the first time or after a fresh setup, you need to initialize the MongoDB replica set. Execute the following command from the `mongo1` container:

    ```bash
    docker exec -it bankingwebapp-mongo1-1 /usr/bin/mongosh --eval 'rs.initiate({ _id: "rs0", members: [ { _id: 0, host: "mongo1:27017" }, { _id: 1, host: "mongo2:27017" }, { _id: 2, host: "mongo3:27017" } ] })'
    ```

    Verify the replica set status:

    ```bash
    docker exec -it bankingwebapp-mongo1-1 /usr/bin/mongosh --eval 'rs.status()'
    ```

4.  **Access the Application:**

    The API will be accessible at `http://localhost:8080`. (Check `docker-compose.yml` for the port)

## Application Structure

* `Api/`: Contains the .NET 8.0 API project.
* `BankingWebApp.Auth/`: Contains the authentication library(simple simulation of authentication).
* `BankingWebApp.Tests/`: Contains the unit tests.
* `docker-compose.yml`: Defines the services, networks, and volumes for the application.
* `Dockerfile`: Defines the Docker image for the API.

## Configuration

* Application settings are in `Api/appsettings.json`.
* MongoDB connection settings are in `Api/Configurations/MongoDbSettings.cs`.
* Authentication settings are in `BankingWebApp.Auth/Configurations/JwtSettings.cs`.

## Running Tests

To run unit tests, navigate to the `BankingWebApp.Tests/` directory and use the .NET CLI:

```bash
dotnet test
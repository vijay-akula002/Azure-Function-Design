# Azure Function SOAP Router Pattern

## Overview

This project implements a **pattern for exposing a single SOAP endpoint
using Azure Functions** that routes incoming SOAP requests to
appropriate backend services.

The Azure Function acts as a **SOAP router and transformer**, allowing
multiple backend SOAP services to be accessed through a **single public
SOAP endpoint**.

### Processing Steps

1.  Receive SOAP request
2.  Extract Action from SOAP Body
3.  Resolve appropriate handler
4.  Transform client request → backend request
5.  Invoke backend SOAP service with authentication token as required
6.  Transform backend response → client response
7.  Return SOAP Reply or SOAP Fault

------------------------------------------------------------------------

# Architecture Diagram

The following diagram shows the **logical architecture** of the SOAP
routing pattern.

``` mermaid
flowchart LR

Client[SOAP Client]

subgraph Azure_Function_App
Router[SOAP Router Function]
Resolver[Handler Resolver]
Handler[Operation Handler]
BackendClient[Backend SOAP Client]
end

BackendService[Backend SOAP Services]

Client --> Router
Router --> Resolver
Resolver --> Handler

Handler --> BackendClient
BackendClient --> BackendService

BackendService --> BackendClient
BackendClient --> Handler
Handler --> Router
Router --> Client
```

------------------------------------------------------------------------

# End-to-End Request Flow

``` mermaid
sequenceDiagram
    participant Client
    participant Function as Azure Function (SOAP Router)
    participant Resolver as Handler Resolver/Service Provider
    participant Handler as Operation Handler
    participant Backend as Backend SOAP Client
	participant Backend as Backend SOAP Service

    Client->>Function: SOAP Request
    Function->>Function: Parse SOAP Envelope
    Function->>Function: Extract Action from Body

    Function->>Resolver: Resolve Handler(Action)
    Resolver-->>Function: Handler Instance

    Function->>Handler: MapRequest(Client SOAP)
    Handler-->>Function: Backend SOAP Request

	Function->>Backend SOAP Client: Invoke Backend SOAP Service
    Backend SOAP Client->>Backend SOAP Service: Invoke Backend SOAP Service
	Backend SOAP Service-->>Backend SOAP Client: Backend SOAP Response	
	Backend SOAP Client-->>Function: Backend SOAP Response
	
    Function->>Handler: MapResponse(Backend Response)
    Handler-->>Function: Client SOAP Response

    Function-->>Client: SOAP Reply / SOAP Fault
```

------------------------------------------------------------------------

# Component Responsibilities

## SOAP Router (Azure Function)

Responsible for:

-   Receiving SOAP request
-   Extracting action from body
-   Resolving correct handler
-   Calling backend service
-   Returning SOAP response
-   Logging and correlation

------------------------------------------------------------------------

## Handler

Handlers implement request/response transformation logic.

Example interface:

``` csharp
public interface IActionHandler
{
    XDocument MapRequest(XDocument clientRequest);
    XDocument MapResponse(XDocument backendResponse);
}
```

Each SOAP operation has its own handler implementation.

Example:

-   ManageNotificationHandler
-   RegisterClientHandler

------------------------------------------------------------------------

## Handler Resolver

Maps the **Action value** in the SOAP body to a handler implementation.

Example mapping:

  Action          Handler
  --------------- ----------------------
  MANAGECLIENTNOTIFICATION  ManageNotificationHandler
  REGISTERCLIENT     		RegisterHandler
  

------------------------------------------------------------------------

# Logging Strategy

## Lower Environments (Dev/Test)

Full payload logging enabled, subjective confirmation that the payloads contain redacted PII

Logged items:

-   Incoming SOAP request
-   Transformed backend request
-   Backend response
-   Final SOAP response

Log level:

DEBUG

------------------------------------------------------------------------

## Production

Only metadata logged.

Metadata includes:

-   Audit ID
-   Correlation ID
-   Transaction ID
-   HTTP status codes
-   Service Results and Subresults
-   Execution time

Log level:

INFO

------------------------------------------------------------------------

# Local Development

## Prerequisites

Install:

-   .NET 8 SDK
-   Azure Functions Core Tools v4
-   Visual Studio or VS Code
-   SoapUI

Verify installation:

    dotnet --version
    func --version

------------------------------------------------------------------------

## Running the Function

Clone repository

    git clone <repo-url>
    cd <repo>

Restore dependencies

    dotnet restore
	
Start the SOAP Mockservice

Start function locally

    func start

Endpoint typically available at

    http://localhost:7071/api/manageparticipant

------------------------------------------------------------------------

## Testing with CURL

    curl -X POST http://localhost:7071/api/SoapRouter -H "Content-Type: text/xml" -d @ManageParticipant.xml

------------------------------------------------------------------------

# Unit Testing

Recommended frameworks:

-   xUnit

Example test structure

    /tests
       Handlers
          ManageNotificationTests.cs
       Router
          FunctionTests.cs

------------------------------------------------------------------------

## Example Handler Test

``` csharp
public class ManageNotificationTests
{
    [Fact]
    public void MapRequest_ShouldTransformClientRequest()
    {
        var handler = new ManageNotificationHandler();

        var clientRequest = XDocument.Parse("<ClientRequest></ClientRequest>");

        var result = handler.MapRequest(clientRequest);

        result.Should().NotBeNull();
    }
}
```

------------------------------------------------------------------------

# Configuration

Environment variables

    BackendBaseUrl
	BackendUri
    BackendTimeout

Secrets are stored in **Azure Key Vault**.

------------------------------------------------------------------------

# Typical Repository Structure

    /Functions
        ManageParticipantFunction.cs

    /Handlers
		/ManageNotification
			Handler.cs
			RequestMapper.cs
			ResponseMapper.cs
			ManageNotificationDto.cs
		/RegisterClient
			Handler.cs
			RequestMapper.cs
			ResponseMapper.cs
			RegisterClientDto.cs
    /Common
        ActionAttribute.cs
        HandlerRegistration.cs
		IActionLabelHandler.cs

    /Models
        ManageParticipant.cs

    /tests
        Handlers
        Router

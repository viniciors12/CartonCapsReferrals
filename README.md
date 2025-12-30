# CartonCapsReferrals

CartonCaps Referrals API

This project implements a mock Referral API service for the CartonCaps app, designed to support referral link generation, referral tracking, and referral resolution during user onboarding. The solution is built using .NET 9, follows clean architecture principles, and includes a documented API contract and unit tests.

API Specification:

The API contract is defined using OpenAPI 3.0. You can find it in the root of the project.
The specification includes:
    Endpoints
    Request / response models
    Error states
    Abuse-prevention rules

Unit tests cover:
    Core service business rules
    Error scenarios
    Controller behavior
    
    dotnet test

Run the API:
    dotnet run
    http://localhost/swagger/index.html

Abuse Prevention Rules:
    Self-referrals are not allowed
    A user can only be a referee once
    A referral can only be resolved once
    Only one active pending referral per referrer

Tech Stack:
    .NET 9
    ASP.NET Core
    xUnit + Moq
    OpenAPI / Swagger

Notes:
    Authentication and user profile management are assumed to be handled by the existing platform.
    Data is stored in-memory for mocking and testing purposes.
    The OpenAPI spec is the source of truth for the API contract.
    Short.io is used as the third-party provider for short link generation.
    In this mock implementation, the API key is stored in `appsettings` for ease of local testing. In a production environment, this value would be managed using a secure secrets provider.






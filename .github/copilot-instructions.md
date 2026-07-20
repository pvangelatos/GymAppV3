# Copilot Instructions

## Project Guidelines
- User prefers command/query pattern: request DTOs should not live in Core or be used in service interfaces; internal command/query models should be in Core and mapped from request models.
- User prefers minimal API endpoint declarations and route mapping to be kept in a separate file from handler implementations (e.g., separate endpoint and handler files).
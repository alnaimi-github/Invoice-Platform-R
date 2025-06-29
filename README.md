# Invoice-Platform-R

## Overview
Invoice-Platform-R is a robust API designed to streamline invoice processing and management. It provides features for creating, updating, exporting, and verifying invoices, making it an essential tool for businesses handling large volumes of invoices.

## Features
- **Invoice Creation**: Upload and create invoices with customer details.
- **Invoice Status Management**: Update invoice and verification statuses.
- **Bulk Export**: Export multiple invoices in various formats with customizable options.
- **Global Exception Handling**: Centralized error handling for consistent and user-friendly error responses.

## Technologies Used
- **Language**: C#
- **Framework**: ASP.NET Core
- **Data Validation**: Data annotations for model validation
- **Middleware**: Custom global exception middleware for error handling

## DTOs
The project includes several Data Transfer Objects (DTOs) for managing invoice-related operations:
- `CreateInvoiceDto`: Handles invoice creation with file upload and customer ID.
- `UpdateInvoiceStatusDto`: Updates the status of an invoice.
- `UpdateVerificationStatusDto`: Updates the verification status of an invoice.
- `BulkExportRequestDto`: Manages bulk export requests with options for format, line items, and tax details.

## Middleware
The project includes a `GlobalExceptionMiddleware` to handle unhandled exceptions and provide standardized error responses.

## Getting Started
1. Clone the repository:
   ```bash
   git clone <repository-url>
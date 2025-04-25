# DDD Asymmetric JWT Rotation: Secure JWT Authentication with Automatic Key Rotation using Domain-Driven Design

## Overview

This project provides a secure, scalable approach to JWT authentication with automatic **asymmetric key rotation**. Designed using **Domain-Driven Design (DDD)** principles, this solution ensures industry-standard security and simplifies key management for modern applications and microservices.

### ✨ Key Features
- **Asymmetric JWT Authentication**  
  Industry-standard public/private key-based signing mechanism for JSON Web Tokens (JWT).

- **Automatic Key Rotation**  
  Private keys are automatically rotated at configurable intervals, ensuring key management is fully automated.

- **Domain-Driven Design (DDD)**  
  Structured with aggregates, value objects, and repositories to enforce clear domain boundaries and maintainable code.

- **Public Key Exposure via JWKS Endpoint**  
  Compliant with RFC 7517 for seamless key distribution to external consumers for token validation.

- **No Manual Key Management**  
  The system handles key generation, rotation, and exposure without requiring manual intervention.

### 🔐 Security Standards
- **RFC 7517**: JSON Web Key (JWK) to expose public keys.
- **RFC 7519**: JSON Web Token (JWT) for authentication.

### 🚀 Ideal Use Cases
- **Microservices**: Secure and scalable token authentication across multiple services.
- **API Authentication**: Public key validation for services exposed to external consumers.
- **Compliance and Security**: Ensures periodic key rotation for high-security systems.

Project base from: https://github.com/evgenirusev/.NET-Domain-Driven-Design-Template

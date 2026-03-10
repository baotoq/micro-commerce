# MicroCommerce — Project Overview

## Purpose
Showcase e-commerce platform demonstrating modern .NET microservices architecture.
Users browse products, add to cart, and checkout via a Next.js storefront.
Backend is a modular monolith designed for gradual extraction to microservices.

## Tech Stack
- **Backend:** .NET 10, ASP.NET Core Minimal APIs, .NET Aspire 13.1.0
- **Gateway:** YARP reverse proxy (CORS, rate limiting, auth, routing)
- **Frontend:** Next.js 16, React 19, TypeScript 5, Tailwind CSS v4, TanStack React Query v5, Radix UI
- **Auth:** Keycloak (backend JWT + NextAuth.js v5 frontend)
- **Database:** PostgreSQL (schema-per-feature, shared `appdb`)
- **Messaging:** Azure Service Bus (MassTransit 9.0) with emulator for dev; RabbitMQ for Kubernetes
- **Patterns:** CQRS (MediatR), DDD, Event-Driven, Vertical Slice Architecture
- **Key Libraries:** Vogen, Ardalis.SmartEnum, Ardalis.Specification, Ardalis.GuardClauses, FluentValidation, FluentResults

## Aspire Topology
PostgreSQL → ApiService → Gateway → Frontend
Frontend resolves API via `services__gateway__https__0` env var.

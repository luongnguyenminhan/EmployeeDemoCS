# Task: Redis Refresh Token Implementation

## Context
File name: 2026-02-20_1_redis-refresh-token.md
Created at: 2026-02-20_10:54:44
Main branch: main
Task Branch: task/add-redis-refresh-token-mgmt_2026-02-20_1
Yolo Mode: Off

## Task Description
Implement Redis-based refresh token storage with device tracking, HMAC-SHA256 hashing, and multi-device support. Migrate from Account entity storage to external Redis cache.

Specifications:
- Device ID: UUID generated server-side, returned to client, required for refresh/logout
- Multi-device: Multiple refresh tokens per user, keyed by `refresh:{userId}:{deviceId}`
- Hashing: HMAC-SHA256 for refresh token storage
- Configuration: Add Redis + JWT token times to appsettings.json
- DB Migration: Remove RefreshToken, RefreshTokenExpiryTime fields
- Logout: Support both single-device and all-devices logout
- API Contract: DeviceId added to TokenModel and ResponseLoginModel

## Project Overview
- Clean Architecture: Domain → Application → Infrastructure → API
- Current auth flow: Stores refresh tokens in Account entity
- Framework: .NET 8+ with ASP.NET Core
- ORM: Entity Framework Core with MySQL
- Current storage: Account.RefreshToken, Account.RefreshTokenExpiryTime
- Docker: MySQL 8.0, will add Redis 7.0

## Analysis
**Current state:**
- Refresh tokens stored directly in Account entity
- No device tracking; only one refresh token per user
- Tokens compared as plaintext
- No automatic expiration; manual cleanup required

**Requirements for change:**
- Redis client: StackExchange.Redis (port 6379)
- Docker compose needs Redis service
- ITokenStore interface (Application layer)
- RedisTokenStore implementation (Infrastructure layer)
- AccountRepository refactoring (login/refresh/logout flows)
- API DTOs updated with DeviceId
- DB migration to remove deprecated fields
- Endpoints updated: /logout, /logout-all-devices added

## Proposed Solution
1. Add Redis service to docker-compose.yml
2. Add StackExchange.Redis NuGet to Application & Infrastructure
3. Create ITokenStore interface (port)
4. Create RedisTokenStore implementation (adapter)
5. Create RedisSettings config binding
6. Update Account entity (remove refresh token fields)
7. Generate EF migration
8. Update DTOs to include DeviceId
9. Update appsettings.json with Redis + JWT times
10. Refactor AccountRepository (login/refresh/logout flows)
11. Update IAccountService + AccountService
12. Register dependencies in DI
13. Update AccountsController endpoints

## Current execution step: "1. Update docker-compose.yml with Redis service"

## Task Progress

### 2026-02-20_10:54:44
- Step: Docker + NuGet + Interfaces + Settings + Redis Implementation
- Status: SUCCESSFUL

### 2026-02-20_15:30:00
- Modified: AuthenTools.cs, GenerateJWTToken.cs, AccountRepository.cs, ClaimsService.cs, AccountsController.cs, IClaimsService.cs, EmployeeDemo.API.http
- Changes: 
  * Added DeviceIdClaimType constant and GetDeviceIdFromClaims() to AuthenTools
  * Updated GenerateJWTToken.CreateToken() to accept deviceId parameter and embed as JWT claim
  * Refactored AccountRepository login flow: generate deviceId before token creation
  * Updated RefreshToken flow to extract deviceId from JWT claims instead of request parameter
  * Added GetDeviceId extraction to ClaimsService
  * Removed [FromQuery] deviceId parameter from refresh-token endpoint
  * Updated logout/logout-all-devices endpoints to extract deviceId from JWT claims
  * Added GetDeviceId property to IClaimsService interface
  * Updated EmployeeDemo.API.http examples with proper endpoint documentation
- Reason: Implement deviceId as immutable JWT claim, eliminate query parameter dependency
- Status: SUCCESSFUL (build verified in Docker)

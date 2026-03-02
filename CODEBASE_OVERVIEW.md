# RBH-1 Codebase Overview

This repository contains a full-stack role-based messaging and analytics app:

- `backend/`: ASP.NET Core Web API + SignalR + EF Core (PostgreSQL) + CouchDB.
- `frontend/`: Angular standalone app (Material UI, SignalR client, Chart.js).

## High-level architecture

### Backend
- Entry point: `backend/Program.cs` wires services, JWT auth, CORS, SignalR, middleware, controllers, and database contexts.
- Data split:
  - **CouchDB** stores user documents (`User`) and session state.
  - **PostgreSQL** stores chat messages (`Message`) through Entity Framework Core.
- Authentication/authorization:
  - JWT token issued by `AuthController` with `Name`, `Role`, and `sessionId` claims.
  - `SessionValidationMiddleware` checks active session against CouchDB on each authenticated request.
- Realtime:
  - `ChatHub` places connections into role-based groups.
  - `MessageController` persists encrypted messages and broadcasts via SignalR.
- Analytics:
  - Admin endpoints aggregate approved users from CouchDB.
  - User endpoints aggregate message activity from PostgreSQL.

### Frontend
- Routing: login, signup, and auth-guarded dashboard.
- Dashboard is role-aware:
  - **Admin** tabs: analytics, messages, pending approvals.
  - **User** tabs: personal analytics and messages.
- Chat uses SignalR for receive and REST for send/history.
- Role and token are persisted in `sessionStorage`.

## Important implementation details

1. **Hybrid persistence is intentional**
   - User lifecycle and approval flow are document-based (CouchDB).
   - Message history and analytics queries are relational (PostgreSQL).

2. **Session invalidation is server-enforced**
   - On login, backend rotates `currentSessionId` in CouchDB.
   - Middleware compares token session claim with DB value, returning 401 if mismatched.

3. **Message privacy model**
   - Messages are encrypted at rest with RSA in backend service.
   - Hash is also stored for integrity-related workflows.
   - API decrypts messages before returning history.

4. **Role-based messaging semantics**
   - Receiver role can be `all`, `admin`, or `user`.
   - SignalR sends to all or role groups; sender also receives own targeted message.

5. **Angular implementation style**
   - Standalone components (no NgModule-centric layout).
   - Dashboard directly orchestrates data loading and chart updates.
   - Some API calls use `HttpClient`, while chat send/history currently use `fetch`.

## How to run locally

1. Start infrastructure:
   - CouchDB on `localhost:5984` with expected credentials.
   - PostgreSQL on `localhost:5432` and DB `LoginSignupChatDB`.
2. Start backend:
   - `cd backend && dotnet run`
3. Start frontend:
   - `cd frontend && npm install && npm start`
4. Open app:
   - `http://localhost:4200`

## Suggested learning path for newcomers

1. **Request lifecycle first**
   - Read `Program.cs` and `SessionValidationMiddleware` to understand auth and pipeline.
2. **Authentication flow**
   - Follow signup/login in frontend + `AuthController` in backend.
3. **Realtime messaging path**
   - Trace `ChatService` -> `MessageController` -> `ChatHub`.
4. **Data model split**
   - Inspect `User` (CouchDB) and `Message` + EF migrations (PostgreSQL).
5. **Dashboard behavior by role**
   - Review `dashboard.ts/html` to see analytics + approvals + chat integration.

## Good next improvements

- Move secrets out of source into environment variables/user-secrets.
- Consolidate frontend network calls around `HttpClient` + centralized interceptors.
- Add stronger password hashing (e.g., BCrypt/Argon2) instead of raw SHA256.
- Consider key management/rotation instead of hardcoded RSA private key.
- Add automated tests for auth/session middleware and dashboard data adapters.

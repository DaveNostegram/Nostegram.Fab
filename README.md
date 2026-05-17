# Flesh and Blood Learning Platform - Architecture Proposal

## Overview

This project is intended to be:

- A learning/tutorial platform for Flesh and Blood
- A lightweight admin/data management platform
- A future statistics/combinatorics platform
- A fun and educational project first
- Potentially expandable later if commercial opportunities appear

The project should prioritize:

- Simplicity
- Fast development
- Strong enough security
- Maintainability
- Learning value
- Avoiding unnecessary enterprise complexity

---

# Core Architecture

## Recommended Stack

### Frontend

- Angular

### Backend

- ASP.NET Core Nostegram.Fab.Api

### Database

- SQL Server or PostgreSQL

### Authentication

- Cookie authentication directly in ASP.NET Core
- No separate IDP initially
- No OAuth/OpenIddict initially
- No browser-held access tokens

---

# High-Level Architecture

```text
Angular Frontend
        ↓
ASP.NET Core Nostegram.Fab.Api
        ↓
Database
```

The Nostegram.Fab.Api will contain:

- Public tutorial endpoints
- Anonymous session/progress endpoints
- Admin-only management endpoints
- Future data/statistics endpoints
- Future multiplayer/game state endpoints

---

# Why No Separate IDP?

A separate Identity Provider is unnecessary at this stage because:

- There is only one frontend
- There is only one Nostegram.Fab.Api
- There are very few authenticated users
- No SSO is required
- No external clients are required
- No delegated authorization is required
- No mobile app currently exists

Using OpenIddict/IdentityServer would add:

- More complexity
- OAuth flows
- Token lifecycles
- Refresh token management
- Client registration
- Redirect URI management
- Additional deployment concerns

These solve problems the project does not currently have.

---

# Product Areas

# 1. Admin Area

## Purpose

The admin area allows:

- Creating cards
- Editing cards
- Managing heroes
- Managing formats
- Managing tutorial scenarios
- Managing data/statistics jobs

## Security

The admin area should require:

- Login
- Admin role

## Recommended Authentication

Use standard ASP.NET Core cookie authentication.

### Flow

```text
Angular Admin Login
        ↓
POST /Nostegram.Fab.Api/auth/login
        ↓
Nostegram.Fab.Api validates credentials
        ↓
Nostegram.Fab.Api issues HttpOnly auth cookie
        ↓
Browser automatically sends cookie
        ↓
Nostegram.Fab.Api authorizes admin requests
```

## Security Notes

Use:

- HttpOnly cookies
- Secure cookies
- SameSite cookies
- Role-based authorization

Example:

```csharp
[Authorize(Roles = "Admin")]
```

---

# 2. Tutorial / Learning Game

## Purpose

This is the primary user-facing feature.

Goals:

- Teach Flesh and Blood
- Interactive tutorials
- Scripted learning scenarios
- Play against tutorial AI/bot
- Possibly multiplayer later

## Initial Security

The tutorial area should initially be:

- Public
- No login required

Do NOT use frontend Nostegram.Fab.Api keys.

Anything in Angular/browser code is public.

---

## Anonymous Progress System

To support:

- Save progress
- Resume tutorial
- Continue across browser refreshes

Use:

- Anonymous session cookies

### Flow

```text
Browser visits tutorial
        ↓
Nostegram.Fab.Api creates anonymous session
        ↓
Anonymous cookie issued
        ↓
Tutorial progress saved against session id
```

This can later evolve into:

- Optional accounts
- Magic link login
- Cross-device saves

---

# Game Architecture

The browser should NOT be authoritative.

Use a server-authoritative model.

## Correct Model

```text
Angular
    = render state + send actions

Nostegram.Fab.Api/Game Engine
    = validate actions + calculate state
```

### Example

```text
Player clicks attack
        ↓
Angular sends action
        ↓
Nostegram.Fab.Api validates legality
        ↓
Game engine updates state
        ↓
Nostegram.Fab.Api returns updated state
        ↓
Angular renders state
```

---

# Multiplayer (Future)

If multiplayer is added later:

Use:

- SignalR/WebSockets
- Server-authoritative state
- Event log/history
- Reconnect handling

Do NOT trust the client to determine game outcomes.

---

# 3. Data Tool

## Purpose

Future statistical tooling:

- Deck combinations
- Hero analysis
- Format analysis
- Probability/statistics exploration

Example:

```text
"There are 59,202,929 possible combinations"
```

---

## Initial Security

The data tool should initially be:

- Admin-only

Reason:

- Queries may become computationally expensive
- Avoid abuse initially

---

## Recommended Architecture

Use:

- Background jobs
- Cached/precomputed results
- Snapshot tables
- Scheduled calculations

Avoid:

```text
User-triggered massive combinatorics queries
```

---

# Authentication Recommendation

# Use Cookie Authentication

Use standard ASP.NET Core cookie auth instead of tokens initially.

## Why?

Because the project is:

- One frontend
- One backend
- Mostly public
- Tiny admin user base

Cookie auth is simpler and easier to maintain.

---

# Recommended Security Model

## Browser

Browser stores:

- HttpOnly auth cookie only

Browser does NOT store:

- Access tokens
- Refresh tokens

---

## Nostegram.Fab.Api

Nostegram.Fab.Api:

- Validates cookies
- Validates admin roles
- Handles sessions directly

---

# Angular vs React

# Recommendation: Angular

Use Angular for this project.

---

# Why Angular Fits This Project

## Team Knowledge

The team already knows Angular.

This is the biggest practical advantage.

---

## Good Fit For

Angular works well for:

- Admin/data-heavy interfaces
- Structured Nostegram.Fab.Applications
- Service-driven architecture
- Complex forms
- Strong consistency across project

---

## Tutorial/Game Side

Angular is still perfectly capable of handling:

- Interactive state
- Game UI
- Animations
- Tutorial flow systems

The project is tutorial-focused, not a high-performance competitive game engine.

---

# When React Might Be Better

React may be preferable if:

- The project becomes highly animation-driven
- The project becomes heavily UI-experiment focused
- The project heavily relies on React ecosystem libraries
- Next.js server rendering becomes important

---

# Final Recommendation

Choose Angular because:

- Existing team knowledge
- Faster delivery
- Easier maintenance
- Cleaner onboarding
- Better fit for admin/data tooling

---

# Future Scalability

If the project grows significantly later:

Possible future additions:

- Separate IDP
- OAuth/OpenIddict
- BFF architecture
- External Nostegram.Fab.Api consumers
- Mobile app
- Commercial integrations

Do NOT build these now.

Build only for current requirements.

---

# Proposed Build Order

# Phase 1 - Foundation

- ASP.NET Core Nostegram.Fab.Api
- Database setup
- Card entity model
- Admin auth

---

# Phase 2 - Admin

- Admin login
- Card CRUD
- Hero management
- Scenario management

---

# Phase 3 - Tutorial System

- Tutorial scenario engine
- Interactive tutorial UI
- Anonymous session system
- Progress save/load

---

# Phase 4 - Game Features

- Bot logic
- State engine
- Rule validation
- Replay/history

---

# Phase 5 - Data Tooling

- Combination/statistics engine
- Cached results
- Snapshot generation
- Admin dashboards

---

# Final Architecture

```text
Angular Frontend
        ↓
ASP.NET Core Nostegram.Fab.Api
        ├── Public Tutorial Nostegram.Fab.Apis
        ├── Anonymous Session Nostegram.Fab.Apis
        ├── Admin Nostegram.Fab.Apis
        ├── Data Tool Nostegram.Fab.Apis
        └── Future Multiplayer Nostegram.Fab.Apis
                ↓
            Database
```

---

# Guiding Principle

Keep the architecture:

- Simple
- Maintainable
- Secure enough
- Easy to evolve

Avoid enterprise complexity until real requirements demand it.
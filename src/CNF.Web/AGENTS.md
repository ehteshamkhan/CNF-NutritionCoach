# NutritionCoach Frontend Engineering Rules

## Product

NutritionCoach is a responsive nutrition application built on:

Angular
Angular Material
ASP.NET Core
SQLite
Canadian Nutrient File

## Architecture

Angular must communicate with nutrition data through the ASP.NET Core API.

Angular must never access SQLite directly.

API:
http://localhost:5285

## UI

The interface should remain:

- vibrant
- modern
- accessible
- responsive
- touch friendly
- keyboard friendly
- visually consistent
- easy to understand

Avoid generic starter-template UI.

## Data integrity

CNF values must not be silently transformed.

In particular:

- null is not zero
- zero is not null
- Food Code is the primary food identifier
- Nutrient Code is interpreted within Food Code
- Measure Code is interpreted within Food Code

## Angular

Prefer:

- standalone components
- signals
- computed signals
- inject()
- built-in @if / @for control flow
- lazy routes
- typed services
- OnPush change detection

Avoid introducing NgModule architecture unless required by a third-party dependency.

## API

The current API is read-only.

Do not add client-side writes to the CNF database.

Current endpoints:

GET /api/health

GET /api/foods/search?q=...

GET /api/foods/{foodCode}

GET /api/foods/{foodCode}/nutrients

GET /api/foods/{foodCode}/measures

GET /api/foods/{foodCode}/measures/{measureCode}

## Design

Primary:
#00A878

Deep primary:
#087F5B

Cyan:
#00A7D2

Coral:
#FF6B6B

Amber:
#FFB703

Violet:
#7048D8

Use color to communicate information, not merely decoration.

## Responsive design

Desktop:
multi-column dashboard

Tablet:
adaptive two-column layout

Mobile:
single-column layout and large touch targets

Never introduce intentional horizontal scrolling.

## Accessibility

Interactive controls must have:

- visible focus states
- meaningful labels
- keyboard operation
- adequate contrast
- appropriate semantic HTML

## Performance

Prefer:

- lazy routes
- typed API responses
- OnPush change detection
- signals for local reactive state
- minimal dependencies

## Do not

Do not:

- expose SQLite to the browser
- hard-code nutrition values into the UI
- fabricate nutrient values
- convert null to zero
- replace CNF source values with estimates
- add external image dependencies without a deliberate product decision

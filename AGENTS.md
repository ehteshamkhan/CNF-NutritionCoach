# CNF NutritionCoach - Agent Instructions

## Project

CNF NutritionCoach is a nutrition-data application built around the
validated Canadian Nutrient File (CNF).

## Architecture

The application follows this architecture:

Angular Frontend
    ->
ASP.NET Core REST API
    ->
SQLite
    ->
Validated Canadian Nutrient File data

Do not bypass the API from the Angular application.

The Angular frontend must never access SQLite directly.

## Backend

Location:

src/CNF.Api

Responsibilities:

- REST API
- SQLite read-only access
- Parameterized SQL
- Food search
- Food details
- Nutrient retrieval
- Measure retrieval

Current API:

http://localhost:5285

Important endpoints:

GET /api/health

GET /api/foods/search?q={query}&limit={limit}

GET /api/foods/{foodCode}

GET /api/foods/{foodCode}/nutrients

GET /api/foods/{foodCode}/measures

GET /api/foods/{foodCode}/measures/{measureCode}

The API is currently read-only.

Do not add POST, PUT, PATCH, or DELETE operations unless explicitly
requested.

## Database

Database:

data/cnf.db

The database contains validated CNF data.

Current validated counts:

Foods: 5,993

Measures: 29,868

Nutrients: 565,409

Do not modify the database manually.

Do not regenerate CNF source data without explicit instruction.

## Data integrity

FoodCode is the food identity.

NutrientCode is unique within a FoodCode.

MeasureCode is unique within a FoodCode.

Do not assume NutrientCode is globally unique.

Do not assume MeasureCode is globally unique.

NULL values are meaningful.

A missing StandardError must remain NULL.

A missing Observations value must remain NULL.

Do not convert NULL to zero.

Actual zero nutrient values must remain zero.

Do not invent nutrition values.

Do not calculate replacement values when the requested value is
missing unless explicitly requested.

## Frontend

Location:

src/CNF.Web

Technology:

- Angular
- Angular Material
- TypeScript
- SCSS
- Standalone components
- Angular routing
- Angular signals where appropriate
- REST API through the CNF API service

## Frontend design

The application should remain:

- Premium
- Vibrant
- Modern
- Responsive
- Accessible
- Fast
- Easy to understand
- Professional rather than childish

Avoid unnecessary visual clutter.

Do not introduce external image dependencies unless explicitly requested.

The application is text/data focused.

## Responsive behavior

The application must work well on:

- Desktop
- Laptop
- Tablet
- Mobile

Do not create fixed-width layouts that break on narrow screens.

Use responsive CSS grids, flexible containers, and mobile-first
behavior where appropriate.

## Accessibility

Maintain:

- semantic HTML
- visible focus states
- keyboard navigation
- accessible labels
- useful ARIA attributes where necessary
- sufficient color contrast
- reduced-motion support

Do not rely on color alone to communicate important information.

## UI data rules

Nutrition values displayed by the frontend must come from the API.

Do not hard-code nutrition values into UI components.

The UI may use hard-coded example search terms, labels, categories,
and explanatory text.

Nutrition numbers must not be fabricated.

## API service

Angular API calls should go through:

src/app/core/services/cnf-api.service.ts

Do not duplicate HTTP logic throughout components.

Use strongly typed interfaces from:

src/app/core/models/food.model.ts

## Routing

Current conceptual routes:

/
 /food/:foodCode

Additional routes may be introduced as the application grows.

## Build

Frontend development:

npm start

Production build:

npx ng build

Backend:

dotnet run

## Validation

After frontend changes:

1. npm install if dependencies changed
2. npx ng build
3. verify the production build succeeds
4. verify API integration
5. verify responsive behavior
6. verify loading and error states

After backend changes:

1. dotnet build
2. run the API
3. test /api/health
4. test search
5. test exact food lookup
6. test nutrients
7. test measures

## Development rules

Prefer small, testable changes.

Do not rewrite working architecture unnecessarily.

Do not replace validated CNF data with sample data.

Do not introduce fake AI functionality.

If a feature is not actually implemented, do not present it as
implemented.

Keep the application truthful about what the current system can do.

## Current known reference test

FoodCode 2:

Cheese souffle

Known API validation values include:

Protein:
9.54415 g per 100 g

MeasureCode 341:

100 ml

Weight:
40.152 g

These values are validation references only.

Do not use them as hard-coded application data.

## Future development direction

Potential future areas include:

- advanced nutrient filtering
- nutrient comparison
- serving-size normalization
- meal planning
- food tracking
- nutrition summaries
- AI-assisted nutrition conversations
- user accounts
- saved foods
- favorites
- richer search
- pagination
- caching
- deployment

These are future capabilities and must not be represented as currently
implemented until they actually exist.

## Core principle

Preserve data integrity first.

Preserve API boundaries second.

Preserve user experience third.

Prefer accurate, maintainable, testable functionality over shortcuts.

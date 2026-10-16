# RaceDay
RaceDay – a database and API design for an event management platform where Organisers create running/walking/cycling events and Participants enrol and view results. Includes ERD, API endpoint plan, and SQL Server schema.

System Description RaceDay is an event management platform for running, walking, and cycling events. Organisers can create events, define the race categories on offer for each event (e.g. 10km, 21km), and attach route/map files to those categories. Participants can browse upcoming events, enrol into a specific category, and view their own results once an Organiser captures them after the race.

The system is modelled around seven core entities: Organiser, Participant, Event, Category, Route, Enrolment, and Result. Organiser and Participant are kept as separate tables (rather than a single Users table with a Role column), so a person's role is determined by which table their account lives in — this is reflected directly in the API's split registration endpoints and role-aware login.

This repository contains the planning artefacts for the project:
Entity Relationship Diagram (ERD) — /docs/ERD.png API Endpoint Plan — /docs/Endpoint-Plan.pdf SQL Database Script — /docs/RaceDayDB.sql
User Roles Role Description Organiser Creates and manages events, defines categories and routes for their events, views enrolments for their own events, and captures participants' results (finish time and position). 
Participant Registers with personal details (including date of birth and gender), browses public events and categories, enrols into a category, and views their own enrolment history and results.
Role is not stored as a flag on a shared table — it's implicit in which table (Organiser or Participant) an account was created in, and the /api/auth/login endpoint checks both tables to determine which role to attach to the session.

CI/CD Pipeline A GitHub Actions workflow (.github/workflows/validate.yml) runs on every push and pull request to main. It validates that:
the /docs folder contains the ERD, Endpoint Plan, and SQL script, and the README contains all required sections.
It optionally also spins up a SQL Server container and executes the SQL script against it to confirm the schema builds cleanly.

YouTube Video: https://youtu.be/8jTy5zKXt9E



# RaceDay API (Part 2)

RESTful ASP.NET Core Web API for the RaceDay running/walking/cycling event system.

![Build and Test](docs/ci-green.png)

**Youtube video:** https://youtu.be/54072iTWuBY

## Technologies
- ASP.NET Core Web API (.NET 8), C#
- SQL Server, Entity Framework Core (Code-First migrations) and SqlClient
- Session-based authentication, BCrypt password hashing
- Swagger (Swashbuckle), xUnit unit tests, GitHub Actions

## The two roles
**Organiser** (ids start with `O`, e.g. O0001)
- Creates, updates and deletes their own events, categories and route maps
- Views enrolments for their own events
- Captures finish times and positions, and views results for their own events

**Participant** (ids start with `P`, e.g. P0001)
- Views events, categories and route maps
- Enrols in a category, views and cancels their own enrolments
- Views their own results

Both roles can register, log in, log out and view or update their own profile.
Guests can only browse events, categories and routes, and register or log in.

## Setup and run
1. Install Visual Studio 2022 with the ASP.NET workload, .NET 8 SDK and SQL Server (or LocalDB).
2. Clone the repository and open `Part2-API/RaceDay.sln`.
3. In `RaceDay/Database/db_connection.cs`, change the `Data source=` value to your SQL Server name.
4. Restore NuGet packages (Build -> Rebuild Solution).
5. Create the database. In Package Manager Console (Default project `RaceDay`) run:
   `Update-Database`
   (The app also applies migrations automatically when it starts.)
6. Set `RaceDay` as the startup project and press F5. Swagger opens in the browser.
7. In Swagger: register an Organiser, log in, create an event and category, log out,
   then register and log in as a Participant and enrol.

## Running the unit tests
- Visual Studio: Test -> Run All Tests
- Command line (from `Part2-API`): `dotnet test`

The tests use in-memory fake databases, so no SQL Server is needed. They also run on every push through GitHub Actions (`.github/workflows/dotnet.yml`).

## Notes
- The database matches the Part 1 ERD and SQL script. Distance is stored on Category.
- Enrol, Create Category and Capture Result take their inputs as query parameters; the other endpoints use a JSON body.
- A participant can enrol in the same category only once (even after cancelling), which returns 409.

# RaceDay
RaceDay – a database and API design for an event management platform where Organisers create running/walking/cycling events and Participants enrol and view results. Includes ERD, API endpoint plan, and SQL Server schema.
System Description RaceDay is an event management platform for running, walking, and cycling events. Organisers can create events, define the race categories on offer for each event (e.g. 10km, 21km), and attach route/map files to those categories. Participants can browse upcoming events, enrol into a specific category, and view their own results once an Organiser captures them after the race.
The system is modelled around seven core entities: Organiser, Participant, Event, Category, Route, Enrolment, and Result. Organiser and Participant are kept as separate tables (rather than a single Users table with a Role column), so a person's role is determined by which table their account lives in — this is reflected directly in the API's split registration endpoints and role-aware login.
This repository contains the planning artefacts for the project:
Entity Relationship Diagram (ERD) — /docs/ERD.png API Endpoint Plan — /docs/Endpoint-Plan.pdf SQL Database Script — /docs/RaceDayDB.sql
User Roles Role Description Organiser Creates and manages events, defines categories and routes for their events, views enrolments for their own events, and captures participants' results (finish time and position). Participant Registers with personal details (including date of birth and gender), browses public events and categories, enrols into a category, and views their own enrolment history and results.
Role is not stored as a flag on a shared table — it's implicit in which table (Organiser or Participant) an account was created in, and the /api/auth/login endpoint checks both tables to determine which role to attach to the session.
CI/CD Pipeline A GitHub Actions workflow (.github/workflows/validate.yml) runs on every push and pull request to main. It validates that:
the /docs folder contains the ERD, Endpoint Plan, and SQL script, and the README contains all required sections.
It optionally also spins up a SQL Server container and executes the SQL script against it to confirm the schema builds cleanly.
YouTube Video: https://youtu.be/8jTy5zKXt9E

PART 2
Updated the ERD, SQL Script, and the API Endpoint Plan

-- =====================================================
-- RaceDay Database - Section C SQL Script (Updated)
-- Organisers and Participants now share one User table.
-- =====================================================

CREATE DATABASE RaceDayDB;
GO

USE RaceDayDB;
GO

-- -----------------------------------------------------
-- 1. USER TABLE
-- UserId format: O0001 (Organiser) / P0001 (Participant)
-- The Role column decides what the user is allowed to do.
-- -----------------------------------------------------
CREATE TABLE [User] (
    UserId        VARCHAR(20)  NOT NULL PRIMARY KEY,
    FullName      VARCHAR(100) NOT NULL,
    Email         VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash  VARCHAR(255) NOT NULL,
    Role          VARCHAR(20)  NOT NULL
                  CHECK (Role IN ('Organiser', 'Participant')),
    DateOfBirth   DATE         NULL,   -- required for Participants only (enforced by the API)
    Gender        VARCHAR(20)  NULL,   -- Participants only

    -- The id prefix must match the role
    CONSTRAINT CK_User_IdMatchesRole CHECK (
        (Role = 'Organiser'   AND UserId LIKE 'O%') OR
        (Role = 'Participant' AND UserId LIKE 'P%')
    )
);
GO

-- -----------------------------------------------------
-- 2. EVENT TABLE
-- OrganiserId must reference a User whose Role = 'Organiser'
-- (checked by the API when the event is created).
-- -----------------------------------------------------
CREATE TABLE Event (
    EventId      INT IDENTITY(1,1) PRIMARY KEY,
    OrganiserId  VARCHAR(20)  NOT NULL,
    EventName    VARCHAR(150) NOT NULL,
    Description  VARCHAR(500) NULL,
    EventDate    DATE         NOT NULL,
    Location     VARCHAR(150) NOT NULL,
    EventType    VARCHAR(20)  NOT NULL DEFAULT 'Run'
                 CHECK (EventType IN ('Run', 'Walk', 'Cycle')),

    CONSTRAINT FK_Event_User FOREIGN KEY (OrganiserId) REFERENCES [User](UserId)
);
GO

-- -----------------------------------------------------
-- 3. CATEGORY TABLE
-- -----------------------------------------------------
CREATE TABLE Category (
    CategoryId    INT IDENTITY(1,1) PRIMARY KEY,
    EventId       INT           NOT NULL,
    CategoryName  VARCHAR(100)  NOT NULL,
    Distance      DECIMAL(6,2)  NOT NULL,

    CONSTRAINT FK_Category_Event FOREIGN KEY (EventId) REFERENCES Event(EventId)
);
GO

-- -----------------------------------------------------
-- 4. ROUTE TABLE
-- A category can have zero or many routes.
-- -----------------------------------------------------
CREATE TABLE Route (
    RouteId     INT IDENTITY(1,1) PRIMARY KEY,
    CategoryId  INT          NOT NULL,
    MapFileUrl  VARCHAR(255) NOT NULL,

    CONSTRAINT FK_Route_Category FOREIGN KEY (CategoryId) REFERENCES Category(CategoryId)
);
GO

-- -----------------------------------------------------
-- 5. ENROLMENT TABLE
-- ParticipantId must reference a User whose Role = 'Participant'
-- (checked by the API when the enrolment is created).
-- -----------------------------------------------------
CREATE TABLE Enrolment (
    EnrolmentId    INT IDENTITY(1,1) PRIMARY KEY,
    ParticipantId  VARCHAR(20) NOT NULL,
    CategoryId     INT         NOT NULL,
    Status         VARCHAR(20) NOT NULL DEFAULT 'Pending'
                   CHECK (Status IN ('Pending', 'Confirmed', 'Cancelled')),
    EnrolledTime   DATETIME    NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Enrolment_User     FOREIGN KEY (ParticipantId) REFERENCES [User](UserId),
    CONSTRAINT FK_Enrolment_Category FOREIGN KEY (CategoryId)    REFERENCES Category(CategoryId),

    -- A participant can only enter the same category once
    CONSTRAINT UQ_Enrolment_Participant_Category UNIQUE (ParticipantId, CategoryId)
);
GO

-- -----------------------------------------------------
-- 6. RESULT TABLE
-- CapturedBy must reference a User whose Role = 'Organiser'
-- (checked by the API when the result is captured).
-- -----------------------------------------------------
CREATE TABLE Result (
    ResultId     INT IDENTITY(1,1) PRIMARY KEY,
    EnrolmentId  INT         NOT NULL UNIQUE,
    CapturedBy   VARCHAR(20) NOT NULL,
    FinishTime   TIME        NULL,
    Position     INT         NULL,

    CONSTRAINT FK_Result_Enrolment FOREIGN KEY (EnrolmentId) REFERENCES Enrolment(EnrolmentId),
    CONSTRAINT FK_Result_User      FOREIGN KEY (CapturedBy)  REFERENCES [User](UserId)
);
GO

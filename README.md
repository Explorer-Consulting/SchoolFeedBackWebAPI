# SchoolFeedBackWebAPI

A school feedback system built with .NET Azure Functions backend and React TypeScript frontend. The system enables teachers to create surveys for the students, collect responses, and generate detailed Excel and PDF reports.

---
## Table of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Prerequisites](#prerequisites)
- [Setup](#setup)
  - [Backend Setup](#backend-setup)
  - [Frontend Setup](#frontend-setup)
- [How to run the application](#how-to-run-the-application)
- [API Documentation](#api-documentation)
---

## Features

### Questionnaire Creation via Excel Upload
Upload structured Excel files containing questions with types, categories, answer options, conditional dependencies, student groups, teacher assignments, and survey dates.

### Questionnaire Management
Delete questionnaires with associated responses and view submission statistics and completion rates.

### Report Generation & Distribution
- **Student Notifications** - Automated email alerts when surveys are available
- **Teacher Reports** - Individual performance analytics (PDF/Excel)
- **Leadership Reports** - Aggregated institutional insights (PDF/Excel)

### Report Types
- **Excel Reports (.xlsx)** - Complete raw data export for in-depth analysis
- **PDF Reports (.pdf)** - Visual statistical summaries with charts for quick insights


### Student Interface
- **Active Questionnaires** - Browse available surveys filtered by subject and teacher
- **Auto-Save Drafts** - Resume incomplete surveys anytime
- **Question Types** - Likert Scale (1-5), Single Choice (with optional "Other"), Multiple Choice, Open-Ended (min 20 chars)
- **User Experience** - Real-time validation with visual feedback and confetti celebration on submission

### Authentication
- **Supported Methods** - Google OAuth 2.0, Facebook Login, Microsoft Authentication, Passwordless Email 
- **Security** - Role-based access control (Admin/Teacher/Student) with email-based admin configuration

---

## Tech Stack

### Backend
- **.NET 9** (isolated worker)
- **Azure Functions** (HTTP triggers, Timer triggers, Queue triggers)
- **Cosmos DB** (NoSQL database)
- **Azure Blob Storage** (report storage)
- **Entity Framework Core**
- **OpenXML** (Excel generation)
- **JWT Authentication**
- **Google OAuth 2.0**

### Frontend
- **React 18** with **TypeScript**
- **Vite** (build tool)
- **shadcn/ui** (UI components)
- **Tailwind CSS**
- **React Router** (navigation)
- **Axios** (HTTP client)
- **React Query** (data fetching)
- **Zustand** (state management)

---

## Prerequisites

Before you begin, ensure you have the following installed:
- **Node.js** (v18 or higher) - [Download](https://nodejs.org/)
- **.NET SDK 9.0** - [Download](https://dotnet.microsoft.com/download)
- **Visual Studio 2026** with **"Azure and AI development"** workload **OR** install:
  - **Visual Studio Code** - [Download](https://code.visualstudio.com/)
  - **Azure Functions Core Tools** (v4) - [Installation Guide](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local)
- **Git**

---

## Setup

### Backend Setup

#### 1. Create `local.settings.json`

Navigate to `FeedBackApp.Backend/AzureFunctionsAPI/` and create a `local.settings.json` file:

```json
{
  "IsEncrypted": false,
  "Values": {
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",

    "Cosmos:AccountEndpoint": "https://studentfeedback-db.documents.azure.com:443/",
    "Cosmos:AccountKey":" Cosmos-Account-key",
    "Cosmos:DatabaseName": "SchoolDatabase",
    "Cosmos:ContainerName": "surveyContainer",

    "Jwt:SecretKey": "JWT-Secret-Key",
    "Jwt:Issuer": "Your-Jwt-Issuer",
    "Jwt:Audience": "Your-Jwt-Audience",

    "Google:ClientId": "Your-Client-Id",

    "Facebook:AppId": "Your-Facebook-Id",
    "Facebook:AppSecret": "Your-Facebook-secret",

    "Microsoft:ClientId": "Your-Microsoft-Id",
    "Microsoft:TenantId": "Your-Microsoft-TenantId",

    "Authorization:AdminEmails": "Admin emails...",
    "Authorization:RequireStudentWhiteList": "true/false",
    "Authorization:UseUniversalStudentGroup": "true/false",

    "SelfOptInJwtOptions:Enabled": "true/false",
    "SelfOptInJwtOptions:Issuer": "Your-SelfOptIn-Issuer",
    "SelfOptInJwtOptions:Audience": "Your-SelfOptIn-Audience",
    "SelfOptInJwtOptions:TokenTtlMinutes": "1440",

    "ReportStorage:ConnectionString": "your-azure-storage-connection-string",
    "ReportStorage:ContainerName": "teacherreports",

    "Email:FromAddress": "feedbackwebapi@gmail.com",
    "Email:FromName": "FeedbackApp",
    "Email:AppPassword": "app-password",

    "Frontend:Url": "https://localhost:5173",
    "Institution:DisplayName": "Your Institution Name",
    "Cors:AllowedOrigins": "https://localhost:5173",

    "Encryption:Key": "Encription-key",
    "Certificates:LoadPath": "C:\\certs\\functions_dev.pfx",

    "Email:BatchSchedule": "****"
  },

  "Host": {
    "CORS": "http://localhost:8080,https://localhost:5173",
    "CORSCredentials": true
  }
}
```

#### 2. Configure Authorization

- **Admin access** — add your email address to the admin list, so you get full admin access:

```json
"Authorization:AdminEmails": "YOUR_EMAIL@gmail.com,...other emails..."
```

  Also set the same value as the `Authorization__AdminEmails` environment variable in:
  **Azure Portal → StudentFeedback-dev-api → Environment variables → App settings → Authorization__AdminEmails**

  Use the **same email** you'll log in with via Google OAuth.

- **Student whitelist enforcement** — `Authorization:RequireStudentWhiteList` controls whether login is restricted to students already present in a survey's student list:
  - `true` (default) — only emails that appear in an uploaded Excel's student sets (or an admin email) can log in.
  - `false` — the whitelist check is skipped at login; any authenticated email is treated as a student. Use this for tenants that rely on self-opt-in instead of pre-uploaded student lists.

  Set the matching `Authorization__RequireStudentWhiteList` environment variable in Azure App Settings as well — this value is **not** derived automatically, it must be set explicitly per environment/tenant.

- **Universal student group** — `Authorization:UseUniversalStudentGroup` is for tenants that have no pre-uploaded student list at all (e.g. a university deployment where every student can self opt-in to every teacher/subject — "everyone is everyone's teacher"):
  - `false` (default) — surveys only get the `StudentSet`s defined by the uploaded Excel.
  - `true` — every new survey automatically gets an additional `"everyone"` `StudentSet`, and every `CreationParam` (teacher/subject pair) is wired to it. Students then gain access purely through self-opt-in (see below) instead of being pre-listed in an Excel-defined `StudentSet`.

  **This requires `Authorization:RequireStudentWhiteList` to be `false`.** If both are `true` at the same time, the app fails to start with an `OptionsValidationException` ("UseUniversalStudentGroup requires RequireStudentWhiteList to be false — otherwise no student could ever log in.").

  Set the matching `Authorization__UseUniversalStudentGroup` environment variable in Azure App Settings as well.

#### 3. Configure Self Opt-In

Self opt-in lets a student gain access to a questionnaire by following a share link and confirming, instead of being pre-listed in a `StudentSet`. It's controlled by the `SelfOptInJwtOptions` section:

- **`SelfOptInJwtOptions:Enabled`** — master switch for the `POST /api/templates/{id}/self-opt-in` endpoint. `false` (or unset) makes the endpoint return `403 Forbidden` for every request, without touching the database.
- **`SelfOptInJwtOptions:Issuer`** / **`SelfOptInJwtOptions:Audience`** — issuer/audience used to validate the short-lived opt-in JWT embedded in the share link.
- **`SelfOptInJwtOptions:TokenTtlMinutes`** — how long a generated share link stays valid. Once expired, opting in returns `410 Gone` ("Invalid or expired link").
- **`SelfOptInJwtOptions:MaxParticipants`** *(optional, `int?`)* — caps how many distinct students can newly opt into a template:
  - Unset / omitted (`null`) — unlimited.
  - `0` — closed to new opt-ins (`403 Forbidden`, "Capacity reached for this template"); students who already opted in keep their existing access.
  - A positive number `N` — the `(N+1)`th distinct student to opt in gets `403 Forbidden`; the first `N` succeed.
  - **A negative number behaves the same as unlimited.** The capacity check only runs when `MaxParticipants >= 0`, so e.g. `-1` silently disables the cap instead of raising a validation error. Leave the key unset rather than using a negative value.
- **`SelfOptInJwtOptions:SigningKey`** — not an independent setting. Even though it is bound from this section, it is unconditionally overwritten at startup with `Jwt:SecretKey`. Setting it directly under `SelfOptInJwtOptions` has no effect — the opt-in JWT is always signed with `Jwt:SecretKey`.

Set the matching `SelfOptInJwtOptions__*` environment variables in Azure App Settings as well.

#### 4. Other Environment-Specific Settings

- **`Frontend:Url`** — the base URL of the deployed frontend. Used to build self-opt-in preview links sent to students.
- **`Institution:DisplayName`** — the school/institution name shown on generated Excel/PDF reports.
- **`Cors:AllowedOrigins`** — the origin(s) allowed to call the API's auth endpoints (comma-separated if multiple). Should match your frontend's URL.

#### 5. Install Dependencies & Build

```bash
cd FeedBackApp.Backend
dotnet restore
dotnet build
```
---

### Frontend Setup

#### 1. Create `.env` File

Navigate to `FeedBackApp.Frontend/` and create a `.env` file:

```env
VITE_GOOGLE_CLIENT_ID=your-google-client-id
VITE_API_BASE_URL=http://localhost:7277/api
VITE_FACEBOOK_APP_ID=your-facebook-app-id
VITE_MICROSOFT_CLIENT_ID=your-microsoft-client-id
VITE_MICROSOFT_TENANT_ID=your-microsoft-tenant-id
```

#### 2. Install Dependencies

```bash
cd FeedBackApp.Frontend
npm install
```

---

## How to run the application

How to get the application running

```bash
# 1. Clone the repository
git clone <REPOSITORY_URL>
cd SchoolFeedBackWebAPI

# 2. Setup Backend
cd FeedBackApp.Backend/AzureFunctionsAPI
# Create local.settings.json (see Backend Setup section)
dotnet restore
dotnet build

# 3. Setup Frontend
cd ../../FeedBackApp.Frontend
npm install

# 4. Run Backend 
cd ../FeedBackApp.Backend/AzureFunctionsAPI
func start

# 5. Run Frontend (in another terminal)
cd ../../FeedBackApp.Frontend
npm run dev
```

**Access the application:**
- Frontend: http://localhost:8080
- Backend API: http://localhost:7277
- Swagger UI: http://localhost:7277/api/swagger/ui
---

##  API Documentation

### Authentication Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/google` | Google OAuth login |
| POST | `/api/auth/otp/send` | Send OTP for email login |
| POST | `/api/auth/otp/verify` | Verify OTP code |
| GET | `/api/optin/share-link/{tid}` | Get opt-in share link |

### Evaluation
| POST | `/api/questionnaire/{id}` | Submit questionnaire response |
| PATCH | `/api/questionnaire/{id}` | Update questionnaire response |


### Questionnaires
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/surveys` | Get user's surveys |
| GET | `/api/surveys/{id}` | Get survey details |
| GET | `/api/management/surveys` | Get all surveys (for Admin) |
| POST | `/api/surveys` | Create new survey |
| DELETE | `/api/surveys/{id:guid}` | Delete survey |

### Templates

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/questionnairetemplate/{id}/preview` | Preview questionnare template event |
| GET | `/api/templates/{id}/preview` | Preview self-opt-in template |
| POST | `/api/templates/{id}/self-opt-in` | Self opt-in to template |

### Reports

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/reports/{templateID}` | Generate Excel report |
| POST | `/api/reports/send/{questionTemplate}` | Send report via email |

### Admin Operations

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/ops/optin/bulk-send-from-db` | Bulk send opt-in emails |

### Documentation

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/swagger/ui` | Swagger UI |
| GET | `/api/swagger.{extension}` | Swagger documentation|
| GET | `/api/openapi/{version}.{extension}` | OpenAPI documentation |

---

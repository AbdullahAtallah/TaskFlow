# TaskFlow backend

The TaskFlow API: teams, projects and tasks, with permissions per role. It follows the FAMAS architecture of Spinnr. Users
register and sign in with their e-mail address and a password; the API issues its own JWT access tokens.

.NET 10 · ASP.NET Core · EF Core 10 on SQL Server · Wolverine 5 · SignalR · FluentValidation · JWT bearer tokens ·
Swagger. Tests use xUnit, AwesomeAssertions, Testcontainers (SQL Server 2022) and Respawn.

## Solution layout

| Project                          | Contents                                                                                                                                   | References          |
|----------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------|---------------------|
| `TaskFlow.Domain`                | Entities, enums, the permission catalogue (packages, features, claims, default roles), exceptions                                          | nothing             |
| `TaskFlow.Application`           | Commands with their validators and handlers, query models and DTOs, the visibility (scope) rules, the password policy, interfaces          | Domain              |
| `TaskFlow.Infrastructure`        | `TaskFlowDbContext`, entity configurations, migrations, seed data, data readers, tokens and password hashing, authorization filters, SignalR | Application, Domain |
| `TaskFlow.Api`                   | `Program.cs`, configuration, controllers                                                                                                   | all of the above    |
| `TaskFlow.Application.UnitTests` | Validators, scope rules, the password policy, the permission catalogue                                                                     |                     |
| `TaskFlow.IntegrationTests`      | The whole API over HTTP, against a real SQL Server                                                                                         |                     |

The Domain, Application and Infrastructure project files fail the build when a reference breaks these rules.

## Getting started

New to the backend, Swagger or JWT? Start with [GETTING_STARTED.md](GETTING_STARTED.md), a short step-by-step guide.

### Prerequisites

- .NET SDK 10.0.401 or a later 10.0.4xx patch (pinned in `global.json`)
- A SQL Server instance: Developer, Express or the Docker image
- The EF Core tools, version 10.0.12: `dotnet tool update --global dotnet-ef --version 10.0.12`
- Docker Desktop, for the integration tests only

### Configuration

Run every command in this README from the `backend` folder.

Secrets never go into `appsettings.json`. Locally they live in the User Secrets of `TaskFlow.Api`; a deployed environment
uses environment variables (`ConnectionStrings__TaskFlowDb`, `Jwt__SigningKey`, and so on).

```powershell
dotnet user-secrets set "ConnectionStrings:TaskFlowDb" "Server=localhost;Database=TaskFlow;Trusted_Connection=True;TrustServerCertificate=True" --project TaskFlow.Api
dotnet user-secrets set "Jwt:SigningKey" "$([Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(48)))" --project TaskFlow.Api
dotnet user-secrets set "Seed:AdministratorEmail" "<your e-mail address>" --project TaskFlow.Api
dotnet user-secrets set "Seed:AdministratorPassword" "<a password of 8+ characters with a letter and a digit>" --project TaskFlow.Api
dotnet user-secrets set "Seed:AdministratorDisplayName" "<your name>" --project TaskFlow.Api
```

The second line generates a random signing key in PowerShell; in bash, use `openssl rand -base64 48`.

| Setting                                         | Where              | Purpose                                                                                                   |
|-------------------------------------------------|--------------------|-----------------------------------------------------------------------------------------------------------|
| `ConnectionStrings:TaskFlowDb`                  | secret, required   | The TaskFlow database. Wolverine keeps its command inbox in the same database, in the `wolverine` schema. |
| `Jwt:SigningKey`                                | secret, required   | Signs and checks the access tokens; at least 32 characters. Whoever knows it can make tokens.            |
| `Seed:AdministratorEmail`                       | first run          | The first administrator, created with the Admin role. Without one nobody can manage users or roles.      |
| `Seed:AdministratorPassword`                    | secret, first run  | That administrator's first password. Set only while the account has none, so it never overwrites a later one. |
| `Seed:AdministratorDisplayName`                 | optional           | Display name of that administrator, if the seeder creates the account.                                   |
| `Jwt:Issuer`, `Jwt:Audience`                    | `appsettings.json` | Written into every token and checked on every request. Both `TaskFlow`.                                  |
| `Jwt:AccessTokenLifetime`                       | `appsettings.json` | How long a token stays valid: `08:00:00`.                                                                 |
| `Calendar:TimeZone`                             | `appsettings.json` | The team's time zone, `Europe/Amsterdam`: "today" for overdue tasks.                                     |
| `RateLimiting:SignInPermitLimit`, `SignInWindow` | `appsettings.json` | Register and login attempts per client IP address: 10 per minute.                                        |
| `RateLimiting:FailedSignInsPerAddress`, `FailedSignInWindow` | `appsettings.json` | Failed sign-ins per e-mail address before it is refused: 5 in 15 minutes.                   |
| `Cors:AllowedOrigins`                           | `appsettings.*.json` | Front-end origins. `appsettings.Development.json` allows `http://localhost:5173`.                        |

The API refuses to start without the connection string or a signing key of at least 32 characters, with an unknown time
zone, or with an administrator password that breaks the password policy.

### Database

```powershell
dotnet ef database update --project TaskFlow.Infrastructure --startup-project TaskFlow.Api
```

This creates the database if it does not exist and applies the migrations in `TaskFlow.Infrastructure/Persistence/Migrations`.
The API never migrates by itself: it refuses to start while a migration is pending, or while the model has changes that no
migration contains yet.

**Seed data.** The roles are seeded the way the Excellence Management System seeds them: `TaskFlow.Infrastructure/Persistence/Seeds.cs`
lists the rows every database starts with, and the entity configurations pass them to EF Core with `HasData`, so the
migrations insert them (`SeedRolesAndPermissions`). That covers the packages, features and claims from
`TaskFlow.Domain/Constants`, and the three default roles with fixed ids and their grants from `DefaultRoleBundles`. After
changing a constant or a seeded role, add a migration; it inserts, updates or deletes exactly the rows that changed.

On every start the API also:

- keeps exactly one role as the default for new users;
- creates the first administrator (`Seed:AdministratorEmail`) with the Admin role and `Seed:AdministratorPassword`. It looks
  after that account only while it has no password: once it has one, later starts leave it alone, and an account that
  somebody else registered with that address is never promoted. If the address already belongs to such an account, choose
  another address. The administrator is not in the seed data, because its address and password are secrets;
- lets Wolverine create or update its own tables in the `wolverine` schema.

A start never changes the seeded roles: what an administrator changes in them stays until a migration changes the same row.

### Run

```powershell
dotnet run --project TaskFlow.Api --launch-profile https
```

The API listens on `https://localhost:7080` and `http://localhost:5080` (plain `dotnet run` uses the first launch profile,
`http`, so only port 5080). In the Development environment Swagger is at `/swagger`, and the root address redirects there;
other environments serve neither. To try the protected endpoints there, call `POST /api/auth/login` with the
administrator's e-mail address and password, click *Authorize* and paste the `accessToken` from the response.

### Tests

```powershell
dotnet test TaskFlow.Application.UnitTests
dotnet test TaskFlow.IntegrationTests
```

The integration tests need Docker. They start one SQL Server 2022 container, apply the migrations and reset the data with
Respawn before every test, so they never touch your own database. Every test signs in with a token from the API's own token
issuer, so every request goes through the real token validation.

## Integrating the frontend

This is the order in which a frontend connects to the API. The CORS policy already allows `http://localhost:5173`.

1. **Register or sign in.** `POST /api/auth/register` with `{ email, displayName, password }` creates an account with the
   role for new users and answers `201`; `POST /api/auth/login` with `{ email, password }` answers `200`. Both return:

   ```json
   { "accessToken": "eyJhbGciOi…", "tokenType": "Bearer", "expiresAt": "2026-10-11T17:30:00Z" }
   ```

   A wrong e-mail address or password answers `401 {"error":"invalid_credentials"}`, a deactivated account
   `401 {"error":"account_revoked"}`, too many attempts `429` (with `{"error":"too_many_attempts"}` after 5 failed
   sign-ins for one address), and form errors `400` with the messages per field (`errors.Email`, `errors.Password`).
   Show those messages under the fields as they are.

2. **Send the token with every request**: `Authorization: Bearer {accessToken}`. An axios request interceptor is the place
   for it. On any `401`, forget the token and go to the sign-in page: the token has expired, the session was ended, or the
   account was deactivated. There are no refresh tokens; after `expiresAt` the user signs in again.

3. **Load the permissions** with `GET /api/user/current`:

   ```json
   {
     "id": "…", "userName": "nina@example.com", "displayName": "Nina",
     "roles": [{ "id": "…", "name": "User" }],
     "packages": ["General", "Projects", "Tasks", "Teams"],
     "features": ["Tasks.Board"],
     "claims": ["Projects.Create", "Projects.Manage", "Tasks.Assign", "Tasks.Board.View", "Tasks.Create", "Tasks.Manage"]
   }
   ```

   Keep it in a context and show or hide with it: menu items by **package**, buttons by **claim**, the board by **feature**
   and claim. The table below lists what each part of the screen needs. Hiding is only a courtesy: the API checks every
   request itself and answers `403` when a permission is missing. Permissions are read from the database on every request,
   so load `/api/user/current` again after a role change or a `403`.

4. **Open the notification connection** to `/notifications` with `@microsoft/signalr`, passing the token through
   `accessTokenFactory`, and listen for `ReceiveNotification`. Most write endpoints answer `202 Accepted` and report the
   outcome there: see [Reads and writes](#reads-and-writes). The connection closes when its token expires; open it
   again after the next sign-in.

5. **Send the row version back when editing.** Teams, tasks and users come with a `rowVersion`; an edit form sends it back
   unchanged in its `PUT`. See [Editing: row versions](#editing-row-versions).

6. **Sign out** with `POST /api/auth/logout` (answers `204`), then forget the token. This ends every session of the user,
   on every device.

| In the frontend                                          | Needs                                              |
|----------------------------------------------------------|----------------------------------------------------|
| Dashboard and profile                                    | a signed-in user (General package)                 |
| Teams menu and team details                              | package `Teams`                                    |
| New team, edit team, delete team                         | `Teams.Manage`                                     |
| Add and remove team members                              | `Teams.Members.Manage`                             |
| Projects menu and project details                        | package `Projects`                                 |
| New project                                              | `Projects.Create`                                  |
| Delete project                                           | `Projects.Manage`                                  |
| Tasks menu, my tasks, task counts                        | package `Tasks`                                    |
| New task                                                 | `Tasks.Create`                                     |
| Edit task (also its status), delete task                 | `Tasks.Manage`                                     |
| Choose or change an assignee                             | `Tasks.Assign`                                     |
| The user picker (team members, assignees)                | `Teams.Manage`, `Teams.Members.Manage` or `Tasks.Assign` |
| Task board                                               | feature `Tasks.Board` and `Tasks.Board.View`       |
| Users and roles screens                                  | package `Management`                               |

## How the API works

### Accounts and sign-in

- Passwords are 8 to 128 characters with at least one letter and one digit. They are stored only as a salted hash (the
  PBKDF2 hasher of ASP.NET Core Identity), never logged, and never put in the message queue: register, login, creating a
  user and setting a password are answered right away instead of through a notification.
- E-mail addresses are compared case-insensitively and stored in lower case; a user's address cannot be changed.
- A token carries the user's id, e-mail address, name and **security stamp**, and is signed with `Jwt:SigningKey` (HMAC-SHA256).
  The API checks the signature, the algorithm, issuer, audience and expiry (no clock skew) on every request.
- Logging out, an administrator setting a new password, and deactivation replace the security stamp: every token issued
  before is refused from then on, also after the user is activated again.
- Register and login allow 10 attempts per minute per client IP address; more answer `429 Too Many Requests`. On top of
  that, 5 failed sign-ins for one e-mail address within 15 minutes, from any number of clients, close that address with
  `429 {"error":"too_many_attempts"}` for the rest of the 15 minutes, even with the right password. Behind a reverse
  proxy, set `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true` (or configure forwarded headers), or every client counts as the
  proxy's address.
- An unknown address and a wrong password get the same answer and take as long, so login does not reveal which addresses
  have an account. Registering does: an address in use answers 400.

Administrators manage accounts under `/api/user`, as far as the design does:

- `POST /api/user` creates a user with an initial password and a role, and answers `201` with the new user.
- `PUT /api/user/{id}/password` sets a new password, for example when someone forgot theirs, and ends their sessions. The
  design has no screen for it: an administrator calls it from Swagger.
- Deactivating a user (`isActive: false`) refuses them at once: `401 {"error":"account_revoked"}` on the next request, and no
  new notification connection; one that is already open closes when its token expires.
- Nobody can deactivate themselves, or take away their own access to user management.

Changing a user's roles, reading one user and deleting a user are commented out in `UserController` and
`UserQueryController`, because the design has no screen for them. See [Endpoints](#endpoints).

### Permissions

A role grants **packages** (modules) and **claims** (single permissions). A user gets everything their roles grant.

| Package    | Claims                                                                                    |
|------------|-------------------------------------------------------------------------------------------|
| General    | none. Every role always has it (dashboard, profile).                                      |
| Teams      | `Teams.View.All`, `Teams.Manage`, `Teams.Members.Manage`                                  |
| Projects   | `Projects.View.All`, `Projects.Create`, `Projects.Manage`                                 |
| Tasks      | `Tasks.View.All`, `Tasks.Create`, `Tasks.Manage`, `Tasks.Assign`, `Tasks.Board.View`      |
| Management | none. The package itself grants user and role management.                                 |

The **Tasks.Board** feature belongs to the Tasks package. Setting `IsEnabled` to false in the `Features` table switches the
board off for everyone, from the next request.

The roles are seeded, as in FAMAS: the seed migration creates three roles, with the same ids in every database. The API
does not create, rename or delete roles; it only changes which packages and claims a role grants (`PUT /api/role/{id}`).

| Role                         | Packages              | Claims                                                                                                   |
|------------------------------|-----------------------|----------------------------------------------------------------------------------------------------------|
| Admin                        | all                   | all                                                                                                      |
| User (default for new users) | all except Management | `Projects.Create`, `Projects.Manage`, `Tasks.Create`, `Tasks.Manage`, `Tasks.Assign`, `Tasks.Board.View` |
| ReadOnlyUser                 | all except Management | `Teams.View.All`, `Projects.View.All`, `Tasks.View.All`, `Tasks.Board.View`                              |

User is the role for new users. Because everyone who registers gets it, it can never grant the Management package.
`PUT /api/role/{id}` takes `{ packages, claims }`, always keeps General, and needs no row version: the role details page
saves every switch on its own, and the latest change wins.

Each controller requires its package (`[Authorization]`) and each action its claim (`[Claim]`). A missing package, feature or
claim answers 403, before the request body is even validated. The user and their permissions are read from the database on
every request, so a role change, a deactivation or a logout applies to the very next request, on every instance of the API.

### What a user can see

| Data    | Visible when                                                                                  |
|---------|-----------------------------------------------------------------------------------------------|
| Team    | you are a member, or you have `Teams.View.All`                                                |
| Project | you are a member of its team, or you have `Projects.View.All`                                 |
| Task    | you are a member of its project's team, or you have `Tasks.View.All` or `Projects.View.All`   |

Commands follow the same rules. Data outside your scope answers 404, and a command on it fails with "not found", exactly
as if it did not exist. Further rules:

- A project can only be created in a team you can see.
- Setting an assignee requires `Tasks.Assign`, and the assignee must be a member of the project's team.
- Removing someone from a team unassigns their tasks in that team's projects.
- A team that still has projects cannot be deleted. Deleting a project deletes its tasks.
- Team names, role names and e-mail addresses are unique among the rows that are not deleted.

The user picker (`GET /api/user/select-list`) lists active users. With `?teamId=` it lists the members of that team, if you
can see the team. Without it, users with `Teams.Manage` or `Teams.Members.Manage` see everyone, and others see the people
who share a team with them.

### Reads and writes

Reads answer `200` with the data.

Most writes (POST, PUT and DELETE on roles, teams, projects and tasks, and the user update) are
commands. The API answers **202 Accepted** without a body, and a Wolverine handler processes the command in the
background. The outcome arrives over SignalR:

- **Hub:** `/notifications`. Give the SignalR client an `accessTokenFactory`; WebSockets send the token as `?access_token=`.
- **Method:** `ReceiveNotification`, with this payload:

```json
{
  "notificationType": "Done",
  "actionType": "Created",
  "entityType": "Team",
  "entityId": "8b7e8f5e-3f3c-4bd6-9b9e-6f1f2a3b4c5d",
  "correlationId": "1d5f3c2a-6b8e-4f1d-9a7c-2e4b6d8f0a1c",
  "message": null
}
```

| Field              | Values                                                                                              |
|--------------------|-----------------------------------------------------------------------------------------------------|
| `notificationType` | `Confirmation` when the command is accepted, then exactly one `Done` or `Error`                     |
| `actionType`       | `BeingProcessed` (with `Confirmation`), `Created`, `Modified`, `Deleted`                            |
| `entityType`       | `User`, `Role`, `Team`, `TeamMember`, `Project`, `Task`                                             |
| `entityId`         | The created or changed item                                                                         |
| `correlationId`    | The same for the `Confirmation` and the `Done` or `Error` of one command                            |
| `message`          | On `Error`: a message for the user, such as "A team with this name already exists."                |

A `Done` is the signal to reload the affected data. A command that fails changes nothing. Rule violations in a request body
(an empty name, a duplicate name, an assignee outside the team) arrive as an `Error` notification, because the handler
validates the command. A body answers 400 only when it cannot be read (malformed JSON, a wrong type, an unknown enum
name) or leaves out a field without a safe default: `isActive`, `status`, `priority` or `rowVersion`.

Register, login, logout, creating a user and setting a password answer right away instead, with `201`, `200` or `204`,
or with the error: `400` with the messages per field, `401`, or `404`.

### Editing: row versions

Every team, project, task, user and role carries a `rowVersion` (a short Base64 string) in the grid rows and the details.
The edits that take it back are `PUT /api/team/{id}`, `PUT /api/task/{id}` and `PUT /api/user/{id}`.
SQL Server gives the record a new version on every save. An edit form sends the version it showed back unchanged in its
`PUT` body. When someone else saved the record in the meantime, the save is refused with the `Error` notification
"Someone else changed this in the meantime. Reload it and try again." and nothing changes; reload the record and edit again.
A `PUT` without `rowVersion` answers 400.

Changing a role's packages and claims, team members, and deletes do not take a version: the latest one wins.

### Grids: search, filters, sorting and paging

All list endpoints take the same parameters, and SQL Server does the filtering, sorting and paging:

```
GET /api/task?query=homepage&status=InProgress&dueFrom=2026-10-01&dueTo=2026-10-31
             &orderByFields[0].field=dueDate&orderByFields[0].dir=Asc
             &orderByFields[1].field=priority&orderByFields[1].dir=Desc
             &skip=0&take=20
```

| Parameter                | Rules                                                                                                    |
|--------------------------|----------------------------------------------------------------------------------------------------------|
| `query`                  | Contains-search, at most 200 characters. Case-insensitive under SQL Server's default collation.          |
| `skip`                   | 0 or more                                                                                                |
| `take`                   | 1 to 100, default 20                                                                                     |
| `orderByFields[i].field` | At most 3 fields from the grid's list below. Another field answers 400 and names the allowed ones.       |
| `orderByFields[i].dir`   | `Asc` or `Desc`                                                                                          |

The response is `{ "data": [ … ], "total": 57 }`, where `total` counts every match before paging. Ties are broken by id,
so the order is stable from page to page, and tasks without a due date sort last in both directions.

| Endpoint                  | Searches               | Filters                                                                                          | Sort fields                                                         |
|---------------------------|------------------------|--------------------------------------------------------------------------------------------------|---------------------------------------------------------------------|
| `GET /api/team`           | name, description      | `onlyMine`                                                                                       | `name`, `memberCount`, `projectCount`, `dateTimeCreated`            |
| `GET /api/project`        | name, description      | `teamId`                                                                                         | `name`, `teamName`, `taskCount`, `openTaskCount`, `dateTimeCreated` |
| `GET /api/task`           | title, description     | `projectId`, `status`, `priority`, `assigneeId`, `unassigned`, `isOverdue`, `dueFrom`, `dueTo`   | `title`, `status`, `priority`, `dueDate`, `dateTimeCreated`         |
| `GET /api/task/my-tasks`  | title, description     | none: always your own open tasks                                                                 | as `GET /api/task`; default `dueDate`                               |
| `GET /api/user`           | name, e-mail address   | `roleId`, `isActive`                                                                             | `displayName`, `userName`, `isActive`, `dateTimeCreated`            |
| `GET /api/role`           | name, description      |                                                                                                  | `name`, `userCount`                                                 |
| `GET /api/role/{id}/user` | name, e-mail address   |                                                                                                  | `displayName`, `userName`, `isActive`                               |

Filters combine. `assigneeId` cannot be combined with `unassigned`, and `dueFrom` cannot come after `dueTo`. A date range is
inclusive and leaves out tasks without a due date. Overdue means not done and due before today, in the team's time zone
(`Calendar:TimeZone`). Invalid parameters answer 400 with validation problem details: an `errors` object with the messages
per parameter.

### Status codes

| Code  | When                                                                                                     |
|-------|----------------------------------------------------------------------------------------------------------|
| 200   | A read, or a login                                                                                       |
| 201   | A registration, or a user created by an administrator                                                    |
| 202   | A command was accepted; the outcome follows over SignalR                                                 |
| 204   | A logout, or a password set by an administrator                                                          |
| 400   | Invalid parameters, a request body that cannot be read or misses a required field, or a broken rule on a direct endpoint |
| 401   | No, invalid or expired token, or an ended session; `{"error":"invalid_credentials"}`; `{"error":"account_revoked"}` |
| 403   | A missing package, feature or claim                                                                      |
| 404   | The item does not exist or lies outside your scope                                                       |
| 409   | A direct endpoint saved over a change made in the meantime                                               |
| 429   | Too many register or login attempts from one client, or `{"error":"too_many_attempts"}` for one address  |
| 500   | An unexpected error, answered with a generic problem details body                                       |

### Data model

```
User ──< UserRole >── Role ──< RolePackage >── Package ──< Feature
                       └────< RoleClaim >──── Claim (belongs to a package, sometimes to a feature)

User ──< TeamMember >── Team ──< Project ──< Task (assignee → User, created by → User)
```

- Users, roles, teams, projects and tasks have `DateTimeCreated`, `DateTimeModified` and `DateTimeDeleted`, stored in UTC,
  and a `RowVersion`. Deleting one sets `DateTimeDeleted`: the row stays in the database but disappears from every query,
  and its name or e-mail address can be used again (filtered unique indexes).
- A user also has a `PasswordHash` (null for an account that cannot sign in with a password) and a `SecurityStamp`.
- Link rows (user roles, team members, role packages, role claims) are deleted for real.
- Task status is `ToDo`, `InProgress` or `Done`; priority is `Low`, `Medium` or `High`. The due date has no time part.

## Endpoints

All routes start with `/api`. Swagger shows every request body; the responses are described in this README.

| Method          | Route                                                             | Requires                                                       |
|-----------------|-------------------------------------------------------------------|----------------------------------------------------------------|
| POST            | `/auth/register`, `/auth/login`                                   | nothing; rate limited                                          |
| POST            | `/auth/logout`                                                    | a signed-in user                                               |
| GET             | `/user/current`                                                   | a signed-in user                                               |
| GET             | `/user/select-list`                                               | `Teams.Manage`, `Teams.Members.Manage` or `Tasks.Assign`       |
| GET             | `/user`                                                           | Management                                                     |
| POST            | `/user`                                                           | Management (with an initial password)                          |
| PUT             | `/user/{id}`                                                      | Management (display name, active)                              |
| PUT             | `/user/{id}/password`                                             | Management (no screen: use Swagger)                            |
| GET             | `/role`, `/role/{id}`, `/role/{id}/user`                          | Management                                                     |
| PUT             | `/role/{id}`                                                      | Management (the role's packages and claims)                    |
| GET             | `/team`, `/team/{id}`, `/team/{id}/member`                        | Teams                                                          |
| POST            | `/team`                                                           | Teams + `Teams.Manage`                                         |
| PUT, DELETE     | `/team/{id}`                                                      | Teams + `Teams.Manage`                                         |
| POST            | `/team/{id}/member`                                               | Teams + `Teams.Members.Manage`                                 |
| DELETE          | `/team/{id}/member/{userId}`                                      | Teams + `Teams.Members.Manage`                                 |
| GET             | `/project`, `/project/{id}`, `/project/counts-per-team`           | Projects                                                       |
| POST            | `/project`                                                        | Projects + `Projects.Create`                                   |
| DELETE          | `/project/{id}`                                                   | Projects + `Projects.Manage`                                   |
| GET             | `/task`, `/task/{id}`, `/task/my-tasks`, `/task/status-counts`    | Tasks                                                          |
| POST            | `/task`                                                           | Tasks + `Tasks.Create`                                         |
| PUT             | `/task/{id}`                                                      | Tasks + `Tasks.Manage`; changing the assignee also `Tasks.Assign` |
| DELETE          | `/task/{id}`                                                      | Tasks + `Tasks.Manage`                                         |
| GET             | `/task/board?projectId=`                                          | Tasks + the Tasks.Board feature + `Tasks.Board.View`           |

`/task/status-counts` and `/project/counts-per-team` feed the dashboard; both count only what you can see.

The API offers what the design (`Task Flow.html`) uses. These endpoints are commented out in their controllers, with
their tests, and come back by removing the `//`:

| Method | Route                     | What it did                                     |
|--------|---------------------------|-------------------------------------------------|
| GET    | `/user/{id}`              | One user with roles and teams                   |
| PUT    | `/user/{id}/permissions`  | Replace a user's roles                          |
| DELETE | `/user/{id}`              | Soft-delete a user                              |
| POST   | `/role`                   | Create a role                                   |
| DELETE | `/role/{id}`              | Delete a role                                   |
| PUT    | `/project/{id}`           | Edit a project                                  |
| PUT    | `/task/{id}/status`       | Change only a task's status                     |
| PUT    | `/task/{id}/assignee`     | Change only a task's assignee                   |

## Development notes

- **New command:** add `TaskFlow.Application/Commands/<Area>/<Name>/` with the command, its validator and its handler. The
  handler validates the command, does the work through the repositories and sends exactly one Done or Error notification;
  it never rethrows. Then add the controller action with its `[Claim(...)]`.
- **A command with a password** is not a message: the controller calls its handler's `Execute` directly and answers right
  away, the handler throws instead of notifying (the exception filter turns that into the status code), and the record
  leaves the password out of `ToString()`. Register these handlers in `TaskFlowApplicationDependencyInjection`.
- **New edit form:** put `RowVersion` in the DTOs and the command (`[property: JsonRequired]`), validate it with
  `RequiredRowVersion()` and call `entity.EnsureUnchangedSince(command.RowVersion)` in the handler.
- **New permission:** add the constant to `ClaimTypes`, and to the default role bundles where it belongs, then add a
  migration (the command under *Schema change*). The migration inserts the claim and grants it to the Admin role and the
  bundles that list it, because `Seeds.cs` builds the seed data from these constants.
- **New default role:** add a bundle to `DefaultRoleBundles` (and its `All` list) and a method with a new fixed id to
  `Seeds.cs` (and to `GetRoleSeed()`), then add a migration.
- **New grid:** derive the query from `FilterParams`, list its sort fields, validate it with `FilterParamsValidator`, and
  page it with `ToFilteringResultAsync` in the data reader.
- **Schema change:** change the entity configuration, then run
  `dotnet ef migrations add <Name> --project TaskFlow.Infrastructure --startup-project TaskFlow.Api --output-dir Persistence/Migrations`.

## Known limitations

- There are no refresh tokens, e-mail verification or self-service password reset: after 8 hours the user signs in
  again, and an administrator sets a new password for someone who forgot theirs, in Swagger.
- Logging out ends every session of that user, on all their devices at once.
- Two role changes handled at the same moment can conflict; the second then fails with an Error notification. The
  frontend waits for the Done of one switch before it sends the next.
- Both sign-in limits are counted in the memory of each API instance. The limit per address also lets anyone lock an
  address out of signing in for 15 minutes by failing on purpose.
- A notification connection that is already open stays open until its token expires, even after a logout, a new
  password or a deactivation; it only carries that user's own command results.

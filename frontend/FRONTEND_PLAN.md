# TaskFlow frontend: implementation plan

This plan says what to build in the frontend, in which order, and how each part talks to the backend.

**Sources**

- `Internship_Task_TaskFlow.docx`: what the app must do.
- `Task Flow.html`: how it looks. Open it in a browser. Its screens are `login`, `register`, `dash`, `teams`, `team`,
  `projects`, `project`, `users`, `roles`, `role` and `profile`.
- `backend/README.md`: every endpoint, with its request and response.

**Follow the design.** Build what `Task Flow.html` shows, no more and no less. The backend offers exactly what the design
uses. Where the plan document asks for something the design leaves out, such as a user details page or a role wizard,
the design wins.

**Two changes from the plan document.**

- **No Auth0.** The backend has its own sign-in: you send an e-mail address and a password, and you get back a JWT access
  token.
- **JavaScript, not TypeScript.** The app stays in JavaScript (`.js` and `.jsx`), as it is today. Ignore the plan
  document's TypeScript requirement and its `.ts` / `.tsx` file names.

---

## How to work through this plan

- Do the phases in order. Each one builds on the one before.
- Each phase has a goal, the permissions it needs, the API calls and a checklist. A phase is done when every box under
  **Test it** is ticked.
- Many pages already exist with static data. In those phases you replace the static data with API calls. You do not
  rebuild the page.
- Test the API in Swagger (`https://localhost:7080/swagger`) before you connect a page to it. Sign in with
  `POST /api/auth/login`, click **Authorize** and paste the `accessToken`.

| #  | Phase                          | Screens                         | Why at this point                                       |
|----|--------------------------------|---------------------------------|---------------------------------------------------------|
| 0  | Setup                          | none                            | Every API call depends on it                            |
| 1  | Sign in and create account     | `login`, `register`             | Without a token, nothing else works                     |
| 2  | Session, guards and app shell  | sidebar, top bar, routes        | Every page needs the user and their permissions         |
| 3  | Profile                        | `profile`                       | The first page with real data. It shows the permissions |
| 4  | Notifications                  | toasts                          | Every create, edit and delete reports its result here   |
| 5  | Teams                          | `teams`, `team`                 | Projects belong to a team                               |
| 6  | Projects                       | `projects`, `project` (header)  | Tasks belong to a project                               |
| 7  | Tasks and board                | `project` (list and board)      | Needs projects                                          |
| 8  | Dashboard                      | `dash`                          | Shows data from phases 5–7 and reuses their dialogs     |
| 9  | Users                          | `users`                         | Admin screens                                           |
| 10 | Roles and permissions          | `roles`, `role`                 | The permission switches, the trickiest screen           |
| 11 | Finish                         | all                             | Tests, loading and error states, demo                   |

---

## How the API works

Read this once. Every phase uses these rules.

1. **The token.** Send `Authorization: Bearer <accessToken>` with every request. A token is valid for 8 hours. There is
   no refresh token: when it expires, the user signs in again.
2. **The permissions.** `GET /api/user/current` returns the user's `packages`, `features` and `claims`.
   - Show or hide menu items by **package**.
   - Show or hide buttons by **claim**.
   - Show the task board only with the **feature** and its claim.

   Hiding is only for the user's convenience. The API checks every request itself and answers `403` when a permission is
   missing.
3. **Reads and writes.**
   - A read (GET) answers `200` with the data.
   - Most writes (POST, PUT, DELETE) answer **`202 Accepted` with no body**. The result arrives later over SignalR as a
     `Done` or `Error` notification (phase 4). On `Done`, reload the data. On `Error`, show the `message` to the user.
   - These answer immediately instead: register, login, logout and creating a user.
4. **Editing.** Teams, tasks and users have a `rowVersion`, a short text. Their edits (the team's General tab, the task
   dialog, Activate / Deactivate) send it back unchanged in the `PUT`. If someone else saved the record in the meantime,
   you get the `Error` notification "Someone else changed this in the meantime. Reload it and try again." Roles need no
   `rowVersion`.
5. **Grids.** Every list endpoint takes the same parameters and answers `{ data: [...], total: 57 }`.
   - `query`: search text.
   - `skip` and `take`: paging. `take` is 1–100.
   - `orderByFields[0].field` and `orderByFields[0].dir`: sorting. `dir` is `Asc` or `Desc`.
   - Each grid also has its own filters, such as `teamId` or `status`.

| Code | Meaning                                         | What the frontend does                                   |
|------|-------------------------------------------------|----------------------------------------------------------|
| 200  | Read or login OK                                | Show the data                                            |
| 201  | Account or user created                         | Continue                                                 |
| 202  | Command accepted; the result follows over SignalR | Wait for `Done` or `Error`                             |
| 204  | Logout                                          | Continue                                                 |
| 400  | Invalid input                                   | Show `errors.<Field>` under each field                   |
| 401  | Not signed in, token expired or account deactivated | Clear the token and go to `/sign-in`                 |
| 403  | Missing permission                              | Show a toast and reload `/api/user/current`              |
| 404  | Not found, or outside your teams                | Show "Not found" and a link back                         |
| 429  | Too many sign-in attempts                       | "Too many attempts. Try again later."                    |

**Enum values** are sent and received as text:

- Task status: `ToDo`, `InProgress`, `Done`. Show them as "To do", "In progress" and "Done".
- Priority: `Low`, `Medium`, `High`.

**Dates.**

- `dueDate` is a date without a time, such as `"2026-10-12"`.
- `dateTimeCreated` is in UTC. Show it with `formatDate()` from `src/utils`.

---

## Accounts to test with

You need three users. They show you what each role sees.

| User          | How to get it                                                                                      |
|---------------|----------------------------------------------------------------------------------------------------|
| Admin         | The first administrator, set up on first run (`backend/README.md` → *Configuration*)               |
| User          | Register through your own Create account page (phase 1). New accounts get the User role            |
| Read-only     | As Admin, create it with the ReadOnlyUser role: the New user form (phase 9), or Swagger `POST /api/user` |

A new user sees no teams, projects or tasks until someone adds them to a team (phase 5, or Swagger
`POST /api/team/{id}/member`).

---

## Phase 0: Setup

**Goal:** one place for API calls, the permission names and the toasts.

**Checklist**

- [ ] Add `frontend/.env.development` with `VITE_API_URL=https://localhost:7080`.
  - The backend allows requests from `http://localhost:5173`, the Vite default.
  - If the browser refuses the certificate, run `dotnet dev-certs https --trust` once, or use `http://localhost:5080`.
- [ ] `npm install @microsoft/signalr`
- [ ] For tests: `npm install -D vitest @testing-library/react @testing-library/jest-dom jsdom`.
- [ ] Create one axios instance in `src/api/client.js`. Every API file uses it.
- [ ] Create `src/constants/permissions.js`. The values must match the backend exactly.
- [ ] Wrap the app in notistack's `<SnackbarProvider>` for toasts.

```js
// src/api/client.js
import axios from "axios";
import { getToken, clearToken } from "../auth/tokenStorage";

export const API_URL = import.meta.env.VITE_API_URL;

const client = axios.create({ baseURL: `${API_URL}/api` });

client.interceptors.request.use((config) => {
  const token = getToken();
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

client.interceptors.response.use(
  (response) => response,
  (error) => {
    const isSignInCall = error.config?.url?.startsWith("/auth/");
    if (error.response?.status === 401 && !isSignInCall) {
      clearToken();
      window.location.assign("/sign-in");
    }
    return Promise.reject(error);
  },
);

export default client;
```

```js
// src/constants/permissions.js
export const Package = {
  General: "General", Teams: "Teams", Projects: "Projects", Tasks: "Tasks", Management: "Management",
};
export const Feature = { TasksBoard: "Tasks.Board" };
export const Claim = {
  TeamsViewAll: "Teams.View.All", TeamsManage: "Teams.Manage", TeamsMembersManage: "Teams.Members.Manage",
  ProjectsViewAll: "Projects.View.All", ProjectsCreate: "Projects.Create", ProjectsManage: "Projects.Manage",
  TasksViewAll: "Tasks.View.All", TasksCreate: "Tasks.Create", TasksManage: "Tasks.Manage",
  TasksAssign: "Tasks.Assign", TasksBoardView: "Tasks.Board.View",
};
```

**Test it**

- [ ] The backend runs, and you can sign in on Swagger as Admin.
- [ ] `npm run dev` starts without errors.

---

## Phase 1: Sign in and create account

**Goal:** a visitor can create an account or sign in, and lands on the dashboard with a token.

**Depends on:** phase 0. **Design:** `login`, `register`. **Permissions:** none; these pages are public.

**API calls**

| Call                       | Send                                  | Get back                                          |
|----------------------------|---------------------------------------|---------------------------------------------------|
| `POST /api/auth/login`     | `{ email, password }`                 | `200 { accessToken, tokenType, expiresAt }`       |
| `POST /api/auth/register`  | `{ email, displayName, password }`    | `201 { accessToken, tokenType, expiresAt }`       |

**Errors to handle**

| Response                                  | Show                                                          |
|-------------------------------------------|---------------------------------------------------------------|
| `401 { error: "invalid_credentials" }`    | The red banner "Wrong e-mail address or password."            |
| `401 { error: "account_revoked" }`        | "This account is deactivated. Contact an administrator."      |
| `429`                                     | "Too many attempts. Try again in 15 minutes."                 |
| `400 { errors: { Email: [...], Password: [...], DisplayName: [...] } }` | The first message under each field |

**Checklist**

- [ ] `src/api/auth/index.js`: `login()`, `register()` and `logout()`.
- [ ] `src/auth/tokenStorage.js`: `saveToken`, `getToken` and `clearToken`.
  - Store `{ accessToken, expiresAt }`.
  - With "Keep me signed in" ticked, use `localStorage`. Otherwise use `sessionStorage`.
  - Treat a token whose `expiresAt` has passed as no token.
- [ ] **Sign in form**
  - Check on the client first: a valid e-mail address and a password that is not empty.
  - Call `login()` and save the token.
  - Go to the page the user first asked for (`location.state.from`), or to `/dashboard`.
  - On a failed sign-in, clear the password field.
- [ ] **Create account form**
  - Check on the client first: a name, a valid e-mail address, and a password of 8–128 characters with at least one
    letter and one digit.
  - Also check that the repeated password matches and that the terms box is ticked. These two never go to the API.
  - Call `register()` and save the token.
  - Go to `/dashboard` and show the info toast "You have the User role. An administrator adds you to a team."
- [ ] **"Forgot password?"**: keep the link as in the design. The backend sends no e-mail, so on a click show the info
      toast "Ask an administrator to reset your password." instead of the design's "A reset link was sent". The
      administrator sets a new password in Swagger with `PUT /api/user/{id}/password` and `{ password }`. There is no
      screen for this in the design.
- [ ] **Footer:** remove "Secured by Auth0" from `AuthFooter.jsx`. TaskFlow no longer uses Auth0. Keep the rest of the
      footer as in the design.
- [ ] When a signed-in user opens `/sign-in` or `/create-account`, send them to `/dashboard`.
- [ ] Disable the submit button while the request runs.

**Test it**

- [ ] Register a new account. You land on the dashboard.
- [ ] Register with the same e-mail address again. The error shows under the e-mail field.
- [ ] Sign in with a wrong password. The banner shows.
- [ ] Enter a wrong password 5 times. The 6th attempt shows the "too many attempts" message.
- [ ] Sign in without "Keep me signed in", then close the browser. You must sign in again.
- [ ] Click "Forgot password?". The toast tells you to ask an administrator.
- [ ] As Admin, set a new password for a user in Swagger. Their open session ends, and only the new password works.

---

## Phase 2: Session, guards and app shell

**Goal:** the whole app knows who is signed in and what they may do. Menus and routes follow the permissions.

**Depends on:** phase 1. **Permissions:** signed in.

**API calls**

| Call                     | Send   | Get back                                                                                 |
|--------------------------|--------|------------------------------------------------------------------------------------------|
| `GET /api/user/current`  | none   | `{ id, userName, displayName, roles: [{ id, name }], packages: [], features: [], claims: [] }` |
| `POST /api/auth/logout`  | none   | `204`. Ends every session of this user, on all devices                                    |

**Checklist**

- [ ] **`CurrentUserProvider`** in `src/context`, with a `useCurrentUser()` hook.
  - It holds `user` and `loading`.
  - It offers `hasPackage(p)`, `hasFeature(f)`, `hasClaim(c)`, `hasAnyClaim(...c)`, `reloadUser()` and `signOut()`.
- [ ] Load `/api/user/current` when the app starts with a token, and again right after sign-in. Show a loader until it
      arrives.
- [ ] **Guards:** `AuthGuard`, `PackageGuard`, `ClaimGuard` and `FeatureGuard`.
  - Without a token, a guard sends the user to `/sign-in` and remembers the page they wanted.
  - Without the permission, it sends them to `/dashboard`.
- [ ] Wrap the routes in the guards, as in the table below.
- [ ] Add a "Page not found" page for `*`.
- [ ] **Sidebar:** build the items from the permissions. Do not hard-code them.
  - "Dashboard" is always there.
  - "Teams" needs package `Teams`.
  - "Projects" needs package `Projects`.
  - The "Management" section, with "Users" and "Roles", needs package `Management`.
- [ ] **Top bar:** show the display name, the role names, the breadcrumb and a link to the profile.
- [ ] **Sign out**
  1. Call `POST /api/auth/logout`. Ignore it if it fails.
  2. Clear the token and the user.
  3. Stop SignalR (from phase 4).
  4. Go to `/sign-in`.
- [ ] On any `403`, show the toast "You don't have permission for this." and call `reloadUser()`. Your roles may have
      changed.

**Routes.** These keep the paths that the app already uses. The plan document uses other names, such as `/login`; either
is fine, as long as you use one set.

| Route                                      | Page                    | Guard                                              |
|--------------------------------------------|-------------------------|----------------------------------------------------|
| `/sign-in`, `/create-account`              | Sign in, create account | public                                             |
| `/dashboard`, `/profile`                   | Dashboard, profile      | signed in                                          |
| `/teams`, `/teams/:id`                     | Teams, team details     | package `Teams`                                    |
| `/projects`, `/projects/:id`               | Projects, project details | package `Projects`                               |
| `/projects/:id/board`                      | Task board              | package `Tasks` + feature `Tasks.Board` + claim `Tasks.Board.View` |
| `/management/users`                        | Users                   | package `Management`                               |
| `/management/roles`, `/management/roles/:id` | Roles, role details   | package `Management`                               |
| `*`                                        | Not found               | public                                             |

```jsx
// Guards are layout routes: they render <Outlet /> or redirect.
<Route element={<AuthGuard />}>
  <Route element={<AppLayout />}>
    <Route path="/dashboard" element={<DashboardPage />} />
    <Route element={<PackageGuard pkg={Package.Teams} />}>
      <Route path="/teams" element={<TeamsPage />} />
      <Route path="/teams/:id" element={<TeamDetailsPage />} />
    </Route>
    {/* ... */}
  </Route>
</Route>
```

**Test it**

- [ ] Signed out, open `/teams`. You go to `/sign-in`. After signing in, you come back to `/teams`.
- [ ] As User, the sidebar has no Management section.
- [ ] As User, typing `/management/users` in the address bar sends you to `/dashboard`.
- [ ] Sign out, then press the browser's Back button. You stay signed out.
- [ ] Deactivate a signed-in user in Swagger. Their next click sends them to `/sign-in`.

---

## Phase 3: Profile

**Goal:** the user sees their account, their teams and their permissions. This makes the permission system visible
while you build the rest.

**Depends on:** phase 2. **Design:** `profile`. **Permissions:** signed in. The teams card also needs package `Teams`.

**API calls**

| Call                                    | Send | Get back                                    |
|-----------------------------------------|------|---------------------------------------------|
| `GET /api/user/current`                 | none | Already loaded in `CurrentUserProvider`     |
| `GET /api/team?onlyMine=true&take=100`  | none | `{ data: [{ id, name, memberCount, projectCount, ... }], total }` |

**Checklist**

- [ ] Replace the static data in `ProfilePage` with `useCurrentUser()`.
- [ ] Show the display name, the e-mail address (`userName`) and the role names.
- [ ] Show the teams card, only with package `Teams`.
- [ ] **Effective permissions:**
  - List the packages. `General` is always there.
  - List the claims, or "No claims" when there are none.
- [ ] Add the sign-out button.

**Test it**

- [ ] Admin, User and ReadOnlyUser each show the packages and claims that `backend/README.md` lists for their role
      (*Permissions* section).
- [ ] Add a user to a team in Swagger, then reload the page. The team appears.

---

## Phase 4: Notifications (SignalR)

**Goal:** every create, edit and delete shows its result as a toast and refreshes the screen.

**Depends on:** phase 2.

**How it works.** A write answers `202` with no body. Shortly after, the hub `/notifications` sends `ReceiveNotification`:

```json
{ "notificationType": "Done", "actionType": "Created", "entityType": "Team",
  "entityId": "…", "correlationId": "…", "message": null }
```

| Field              | Values                                                                              |
|--------------------|-------------------------------------------------------------------------------------|
| `notificationType` | `Confirmation` first, then one `Done` or one `Error`                                |
| `actionType`       | `BeingProcessed`, `Created`, `Modified`, `Deleted`                                  |
| `entityType`       | `User`, `Role`, `Team`, `TeamMember`, `Project`, `Task`                             |
| `message`          | On `Error`: a sentence for the user, such as "A team with this name already exists." |

**Checklist**

- [ ] **`NotificationProvider`**
  - Opens one connection after sign-in, and stops it on sign-out.
  - The connection closes when the token expires. Open it again after the next sign-in.
- [ ] **On `Done`:** show a success toast, such as "Team created" (built from `entityType` and `actionType`).
- [ ] **On `Error`:** show an error toast with `message`.
- [ ] **On `Confirmation`:** do nothing, or show "Saving…".
- [ ] **A `useNotification(entityTypes, onDone)` hook.** A page uses it to reload its data, for example
      `useNotification(["Team", "TeamMember"], reload)`.
- [ ] **Forms:**
  - Close a dialog when its request returns `202`. The toast reports the outcome.
  - When the `Error` is about a stale `rowVersion`, reload the record.

```js
import { HubConnectionBuilder } from "@microsoft/signalr";

const connection = new HubConnectionBuilder()
  .withUrl(`${API_URL}/notifications`, { accessTokenFactory: () => getToken() })
  .withAutomaticReconnect()
  .build();

connection.on("ReceiveNotification", (notification) => { /* toast + notify the subscribers */ });
await connection.start();
```

**Test it**

Notifications go to every open connection of the user who sent the command. So for these tests, sign in to Swagger
with the same account as the app.

- [ ] Create a team in Swagger while the app is open. The app shows the toast.
- [ ] Create a second team with the same name. The app shows the error message.

---

## Phase 5: Teams

**Goal:** teams overview, create wizard, and a details page with General, Members and Projects tabs.

**Depends on:** phase 4. **Design:** `teams`, `team`, and the `team` (wizard), `member` and `confirm` modals.

**Permissions**

| What                                          | Needs                                    |
|-----------------------------------------------|------------------------------------------|
| Teams page and team details                   | package `Teams`                          |
| "New team", the Edit and Delete row actions, editing the General tab | `Teams.Manage`. Without it, show the fields disabled |
| "Add members", and "Remove" on a member       | `Teams.Members.Manage`                   |
| Projects tab                                  | package `Projects`                       |
| Subtitle "All teams in the organisation"      | `Teams.View.All`. Otherwise "The teams you are a member of" |

**API calls**

| Call                                       | Send                                     | Get back                                   |
|--------------------------------------------|------------------------------------------|--------------------------------------------|
| `GET /api/team`                            | `query`, `skip`, `take`, sort by `name`, `memberCount`, `projectCount` or `dateTimeCreated` | `{ data: TeamRow[], total }` |
| `GET /api/team/{id}`                       | none                                     | `{ id, name, description, memberCount, projectCount, dateTimeCreated, rowVersion }` |
| `GET /api/team/{id}/member`                | none                                     | `[{ userId, displayName, userName, isActive, roles: [] }]` |
| `GET /api/project?teamId={id}`             | none                                     | The Projects tab                           |
| `GET /api/user/select-list?query=&take=50` | none                                     | `[{ id, displayName, userName }]`, for the member picker |
| `POST /api/team`                           | `{ name, description, memberIds: [] }`   | `202`                                      |
| `PUT /api/team/{id}`                       | `{ name, description, rowVersion }`      | `202`                                      |
| `DELETE /api/team/{id}`                    | none                                     | `202`                                      |
| `POST /api/team/{id}/member`               | `{ userIds: [] }`                        | `202`                                      |
| `DELETE /api/team/{id}/member/{userId}`    | none                                     | `202`                                      |

**Build these once and reuse them later**

- **`useServerGrid`**: holds the page, sort and search, and turns them into `skip`, `take` and `orderByFields`.
  - Use MUI X `DataGrid` with `paginationMode="server"` and `sortingMode="server"`.
  - Pass `total` as `rowCount`.
  - Wait 300 ms after the last keystroke before searching.
- **`ConfirmDialog`**, a **save bar** ("You have unsaved changes. Discard / Save"), **tabs**, and a **user picker**.

```js
// DataGrid's state → API parameters
export const toGridParams = ({ page, pageSize }, sortModel, extra = {}) => {
  const params = { skip: page * pageSize, take: pageSize, ...extra };
  sortModel.forEach((sort, i) => {
    params[`orderByFields[${i}].field`] = sort.field;
    params[`orderByFields[${i}].dir`] = sort.sort === "asc" ? "Asc" : "Desc";
  });
  return params;
};
```

**Checklist**

- [ ] **Teams overview:** a grid with server-side search, sorting and paging. Click a row, or its "Edit" action, to open
      `/teams/:id`. "Delete" asks for confirmation.
- [ ] **New team wizard:** Details → Members → Summary.
  - Name: required, at most 100 characters.
  - Description: at most 4000 characters.
  - Add the current user to the members by default, as the design does. Otherwise a creator without `Teams.View.All`
    cannot see their own team.
- [ ] **Details, General tab:** edit the name and description, then save with the save bar. Send the `rowVersion` you
      loaded.
- [ ] **Delete:** put the delete button in a danger section, and ask for confirmation.
  - While `projectCount > 0`, disable the button and show "This team still owns N active projects. Move or delete them
    first."
- [ ] **Members tab:** list the members, add members, and remove a member.
  - The picker hides people who are already members.
  - Removing someone also unassigns their tasks in this team's projects. Say so in the confirmation.
- [ ] **Projects tab:** list the team's projects. Click a row to open the project.
- [ ] On `Done` for `Team` or `TeamMember`, reload the grid or the details.

**Test it**

- [ ] As Admin: create, rename and delete a team, and add and remove members.
- [ ] As User: there is no "New team" button and no row actions, and the General tab is read-only.
- [ ] As User, call `POST /api/team` in Swagger. You get `403`.
- [ ] A user who is not a member opens `/teams/<id>` for that team. They get "Not found" (the API answers `404`).
- [ ] Edit the same team in two browser tabs and save both. The second save shows the "Someone else changed this" error.

---

## Phase 6: Projects

**Goal:** projects overview with a team filter, and creating and deleting projects. Also the header of the project
details page. As in the design, projects are not edited.

**Depends on:** phase 5. **Design:** `projects`, the top of `project`, the `project` modal and `confirm`.

**Permissions**

| What                                  | Needs                                              |
|---------------------------------------|----------------------------------------------------|
| Projects page and project details     | package `Projects`                                 |
| "New project"                         | `Projects.Create`                                  |
| The "Delete" row action               | `Projects.Manage`                                  |
| Subtitle "Projects of every team"     | `Projects.View.All`. Otherwise "Projects of your teams" |

**API calls**

| Call                                  | Send                                      | Get back                                  |
|---------------------------------------|-------------------------------------------|-------------------------------------------|
| `GET /api/project`                    | `teamId`, `query`, `skip`, `take`, sort by `name`, `teamName`, `taskCount`, `openTaskCount` or `dateTimeCreated` | `{ data: ProjectRow[], total }` |
| `GET /api/project/{id}`               | none                                      | `{ id, name, description, teamId, teamName, taskCount, openTaskCount, doneTaskCount, rowVersion, ... }` |
| `GET /api/project/counts-per-team`    | none                                      | `[{ teamId, teamName, projectCount }]`: the teams for the filter tabs and the "Team" dropdown |
| `POST /api/project`                   | `{ teamId, name, description }`           | `202`                                     |
| `DELETE /api/project/{id}`            | none                                      | `202`. Its tasks are deleted too          |

**Checklist**

- [ ] **Overview grid**
  - Filter tabs: "All teams", then one tab per team.
  - The "Open tasks" column shows `openTaskCount`.
  - The "Done" column shows `doneTaskCount / taskCount`.
  - Mark rows with `hasOverdueTasks`.
- [ ] **New project dialog:** Team, Name (required, at most 100 characters) and Description.
  - Preselect the team of the active filter tab.
- [ ] **Delete:** confirm with "<name> and all of its tasks will be deleted."
- [ ] **Details page header**
  - Show the name, then "team name · description".
  - Breadcrumb: Projects › Team › Project. Link the team only with package `Teams`.
- [ ] On `Done` for `Project`, reload.

**Test it**

- [ ] As User: create a project in your team, then delete it.
- [ ] As ReadOnlyUser: no "New project" button and no "Delete" action.
- [ ] A member of team A cannot open a project of team B (`404`), unless they have `Projects.View.All`.

---

## Phase 7: Tasks and the task board

**Goal:** the task list on the project details page, with filters, an add and edit dialog, delete, and the board view.
The dialog saves every field at once, the status and the assignee included.

**Depends on:** phase 6. **Design:** `project` (List and Board), the `task` modal and `confirm`.

**Permissions**

| What                                       | Needs                                                      |
|--------------------------------------------|------------------------------------------------------------|
| The task list                              | package `Tasks`                                            |
| "New task"                                 | `Tasks.Create`                                             |
| The "Edit" and "Delete" row actions        | `Tasks.Manage`                                             |
| Choosing or changing the assignee          | `Tasks.Assign`. Without it, show the pills disabled with the hint "Requires Tasks.Assign" |
| The List / Board switch                    | feature `Tasks.Board` + claim `Tasks.Board.View`           |

**API calls**

| Call                                   | Send                                                                          | Get back                     |
|----------------------------------------|-------------------------------------------------------------------------------|------------------------------|
| `GET /api/task`                        | `projectId`, `status`, `priority`, `assigneeId` or `unassigned=true`, `skip`, `take`, sort by `dueDate`, `title`, `status`, `priority` or `dateTimeCreated` | `{ data: TaskRow[], total }` |
| `GET /api/task/{id}`                   | none                                                                          | The full task, with `description` and `rowVersion` |
| `GET /api/user/select-list?teamId={teamId}` | none                                                                     | The project team's members, for the assignee picker. Needs `Tasks.Assign` |
| `POST /api/task`                       | `{ projectId, title, description, status, priority, dueDate, assigneeId }`    | `202`                        |
| `PUT /api/task/{id}`                   | `{ title, description, status, priority, dueDate, assigneeId, rowVersion }`   | `202`                        |
| `DELETE /api/task/{id}`                | none                                                                          | `202`                        |
| `GET /api/task/board?projectId={id}`   | Optional: `priority`, `assigneeId`, `unassigned`                              | `{ projectId, projectName, columns: [{ status, count, tasks: [TaskCard] }] }` |

**Checklist**

- [ ] **Filters**
  - Status: All, To do, In progress, Done.
  - Priority: All, High, Medium, Low.
  - Assignee: Everyone, Me (`assigneeId` = your own `id`), Unassigned (`unassigned=true`).
- [ ] **Task list**
  - Sort by due date by default.
  - For overdue tasks (`isOverdue`), show "N days overdue" in red.
  - Show "No tasks match these filters." when the list is empty.
- [ ] **Task dialog:** Title (required, at most 200 characters), Description, Status, Priority, Due date (optional, sent
      as `"yyyy-MM-dd"`) and Assignee.
  - **Create:** send `assigneeId: null` when the user has no `Tasks.Assign`.
  - **Edit:** load the task with `GET /api/task/{id}` first, because the list rows have no `description`. Without
        `Tasks.Assign`, send the current `assigneeId` unchanged. Changing it, or clearing it, without that claim fails.
- [ ] **Delete:** with confirmation.
- [ ] **Board:** the List / Board switch moves between `/projects/:id` and `/projects/:id/board`.
  - Three columns, with a count and cards: title, priority, due date and assignee.
  - Clicking a card opens the edit dialog, with `Tasks.Manage` only.
- [ ] On `Done` for `Task`, reload the list or the board.

**Test it**

- [ ] As User: create a task with an assignee from the team, edit it (also its status) and delete it.
- [ ] In Swagger, save a task with an assignee who is not in the team (`PUT /api/task/{id}`). The app shows the `Error`
      toast.
- [ ] As ReadOnlyUser: the list and board are visible, and nothing can be changed.
- [ ] Switch the board off with `Features.IsEnabled = 0` in the database. The switch disappears after a reload, and
      `/projects/:id/board` sends you to the dashboard.

---

## Phase 8: Dashboard

**Goal:** the landing page, with a greeting and one card per package.

**Depends on:** phases 5–7, for the data and the New project and New task dialogs. **Design:** `dash`.

**Permissions**

| Card or button                                | Needs                                               |
|-----------------------------------------------|-----------------------------------------------------|
| "My tasks" and "Tasks per status"             | package `Tasks`                                     |
| "Projects per team"                           | package `Projects`                                  |
| "My teams"                                    | package `Teams`                                     |
| "New project"                                 | `Projects.Create`                                   |
| "New task"                                    | `Tasks.Create`, and at least one visible project    |

**API calls**

| Call                                      | Send | Get back                                              |
|-------------------------------------------|------|-------------------------------------------------------|
| `GET /api/task/my-tasks?take=6`           | none | `{ data: TaskRow[], total }`: your open tasks, by due date |
| `GET /api/task/status-counts`             | none | `{ toDo, inProgress, done, overdue }`                  |
| `GET /api/project/counts-per-team`        | none | `[{ teamId, teamName, projectCount }]`                 |
| `GET /api/team?onlyMine=true&take=100`    | none | Your teams, with `memberCount` and `projectCount`      |

**Checklist**

- [ ] **Greeting:** "Welcome back, {displayName}", with the subtitle "{total} open tasks assigned to you · {today}".
- [ ] **My tasks:** title, project, due date (or "N days overdue") and priority. Click a task to open its project.
      Show "No open tasks assigned to you." when there are none.
- [ ] **Tasks per status:** four tiles: To do, In progress, Done, Overdue.
- [ ] **Projects per team:** a bar for each team.
- [ ] **My teams:** "N members · N projects". Click a team to open it.
- [ ] The sidebar badge next to "Dashboard" shows the `total` from my-tasks.
- [ ] Each card loads on its own, with its own loader, so one slow card does not block the rest.
- [ ] On `Done` for `Task`, `Project` or `Team`, reload the cards.

**Test it**

- [ ] The numbers match what you see on the Projects page and in the task lists for the same user.
- [ ] A user without package `Teams` does not see "My teams". To test this, switch `Teams` off for the ReadOnlyUser role
      in Swagger (`PUT /api/role/{id}`), sign in as a read-only user, and switch it back on afterwards.
- [ ] A brand-new user sees empty cards, not errors.

---

## Phase 9: Users (Management)

**Goal:** administrators see all users, create a user, and activate or deactivate a user. That is all the design has:
there is no user details page.

**Depends on:** phase 4, and the grid from phase 5. **Design:** `users` and the `user` modal.

**Permissions:** package `Management` for everything in this phase.

**API calls**

| Call                              | Send                                                  | Get back                                     |
|-----------------------------------|-------------------------------------------------------|----------------------------------------------|
| `GET /api/user`                   | `query`, `roleId`, `isActive`, `skip`, `take`, sort by `displayName`, `userName`, `isActive` or `dateTimeCreated` | `{ data: [{ id, userName, displayName, isActive, roles, teams, rowVersion }], total }` |
| `GET /api/role?take=100`          | none                                                  | The roles for the Role field                 |
| `POST /api/user`                  | `{ userName, displayName, password, roleIds: [roleId] }` | **`201`** with the new user. Answers right away; errors come back as `400` |
| `PUT /api/user/{id}`              | `{ displayName, isActive, rowVersion }`               | `202`                                        |

**Checklist**

- [ ] **Users grid:** Name, E-mail, Roles, Teams and Status, as in the design.
- [ ] **Row action Deactivate / Activate:** `PUT /api/user/{id}` with the row's `displayName`, the new `isActive`, and
      the row's `rowVersion`.
  - Hide it on your own row, as the design does. The API refuses it anyway.
- [ ] **New user dialog:** Name, E-mail address, **Password** and Role.
  - The password field is the one addition to the design: users sign in with a password, so the administrator chooses
    the first one and passes it on.
  - Use the same password rules as Create account: 8–128 characters, at least one letter and one digit.
  - The Role field picks one role. Send it as `roleIds: [roleId]`.
  - Show the `400` errors (`errors.UserName`, `errors.DisplayName`, `errors.Password`, `errors.RoleIds`) under the
    fields.
  - On `201`, close the dialog, show the toast "{name} can now sign in." and reload the grid.

**Test it**

- [ ] Create a user with the ReadOnlyUser role, then sign in as them in a private window. They see no buttons that
      change data.
- [ ] Deactivate them. Their next click goes to `/sign-in`. Activate them, and they can sign in again.
- [ ] Your own row has no Deactivate action.
- [ ] As User, call `GET /api/user` in Swagger. You get `403`.

---

## Phase 10: Roles and permissions (Management)

**Goal:** list the roles, switch a role's packages and claims on and off, and see who has the role.

The roles are fixed: Admin, User and ReadOnlyUser come with the database. As in the design, there is no "New role"
button, and roles are not renamed or deleted.

**Depends on:** phase 9. **Design:** `roles`, and `role` with the Permissions and Assigned users tabs.

**Permissions:** package `Management` for everything in this phase.

**API calls**

| Call                        | Send                                    | Get back                                   |
|-----------------------------|-----------------------------------------|--------------------------------------------|
| `GET /api/role`             | `query`, `skip`, `take`, sort by `name` or `userCount` | `{ data: [{ id, name, description, isDefault, packageCount, claimCount, userCount }], total }` |
| `GET /api/role/{id}`        | none                                    | The same, plus `packages: []` and `claims: []` |
| `GET /api/role/{id}/user`   | `query`, `skip`, `take`                 | `{ data: [{ id, displayName, userName, isActive }], total }` |
| `PUT /api/role/{id}`        | `{ packages: [], claims: [] }`: the full lists after the switch | `202`                |

**The catalogue.** There is no endpoint for the list of packages and claims. Keep it in the frontend as a constant,
next to `permissions.js`. Copy the `CATALOGUE` from `Task Flow.html`: each package with its description, its claims and
their descriptions.

**Rules for the permission switches**

- **Each switch saves at once**, as in the design. Send the role's full `packages` and `claims` lists, with the change
  applied. No `rowVersion` is needed.
- **One save at a time.** Disable the role's switches until the `Done` or `Error` of the last switch arrives. Two saves
  at the same moment can clash.
- **On `Error`**, show the message and reload the role, so the switch flips back.
- `General` is always on and locked.
- Switching a claim on also switches its package on. Switching a package off also switches its claims off. The API
  refuses a claim without its package.
- `Tasks.Board.View` shows a small "Tasks.Board" feature label.
- **Ignore one rule from the plan document:** "switching on X.Manage also switches on X.View". In TaskFlow, `X.View.All`
  means "see the data of every team". It is not needed for Manage: the User role has `Projects.Manage` without
  `Projects.View.All`.

**Checklist**

- [ ] **Roles grid:** Role, Description, number of packages, claims and users, and a "Default" tag on the User role.
      Click a row to open `/management/roles/:id`.
- [ ] **Permissions tab:** one card per package, each with its claims as switches, following the rules above.
- [ ] **Assigned users tab:** a paged grid with Name, E-mail and Status.
- [ ] After changing a role that you have yourself, call `reloadUser()`.

**Test it**

- [ ] Switch `Projects.Create` off for the User role. A signed-in User loses "New project" after a reload, and
      `POST /api/project` answers `403` on their very next call. Switch it back on.
- [ ] Switch `Management` on for the User role. The `Error` toast says the role for new users cannot grant it, and the
      switch flips back.
- [ ] As Admin, switch `Management` off for the Admin role. The `Error` toast says you cannot remove your own access.
- [ ] Click two switches quickly, one after the other. Both changes are saved.

---

## Phase 11: Finish

**Goal:** the app is solid, tested and ready for the demo.

**Checklist**

- [ ] **Every page has:**
  - a loading state (skeleton or spinner);
  - an empty state;
  - an error state with a retry.
- [ ] **No console errors or warnings** during normal use. `npm run lint` passes.
- [ ] **Atomic Design:** pages use the components in `src/components`, never MUI directly.
- [ ] **Tests (Vitest + React Testing Library):**
  - the guards: redirect without a token, and redirect without a package or claim;
  - the permission helpers;
  - sign-in and create-account validation;
  - one atom, one molecule and one organism each.
- [ ] **Storybook stories** for the atomic components. The plan document asks for them.
- [ ] **README:** add a frontend section with `VITE_API_URL`, `npm install` and `npm run dev`, and the three test
      accounts.
- [ ] **Demo, in this order:**
  1. Create an account and sign in.
  2. Show the dashboard.
  3. Create a team and add members.
  4. Create a project and tasks.
  5. Switch a claim of a role, and show the UI and the API change.
  6. Show a `403` from the API.
  7. Sign out.

**Bonus, if time allows**

- Drag and drop on the board with dnd-kit. A move changes the status and needs `Tasks.Manage`. It needs
  `PUT /api/task/{id}/status`, which is commented out in the backend's `TaskController`: uncomment it first.
- Playwright end-to-end tests.
- Dark mode.

---

## Done when

- [ ] A new user can create an account, sign in, and land on the dashboard with the User role's permissions.
- [ ] Signing out goes to `/sign-in`. A protected page without a session also goes to `/sign-in`.
- [ ] Menus, pages, buttons and row actions match the user's permissions. A guarded URL typed by hand goes to
      `/dashboard`.
- [ ] Everything the design shows works against the API, with Done and Error toasts:
  - teams: create, edit, delete, and add and remove members;
  - projects: create and delete;
  - tasks: create, edit and delete;
  - users: create, activate and deactivate;
  - roles: switch packages and claims.
- [ ] A change to a role shows in the UI after a reload.
- [ ] There are no console errors, and the tests pass.

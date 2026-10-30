# TaskFlow – Frontend

The web app for TaskFlow: teams, projects and tasks, with roles and permissions.

Built with React 19 and Vite, MUI, React Router 7 and notistack, and organised
with Atomic Design (`src/components/atoms`, `molecules`, `organisms`, `templates`).
Pages only use components from `src/components`, never MUI directly.

## Requirements

- Node.js 20 or newer
- The TaskFlow API running on `http://localhost:5080` (see the backend README)

## Setup

1. Create `.env.development` in this folder with the API address:

```
   VITE_API_URL=http://localhost:5080
```

2. Install the packages and start the app:

```bash
   npm install
   npm run dev
```

3. Open http://localhost:5173 and sign in with one of the test accounts.

## Test accounts

The **Admin** account is created by the API on its first start, from user secrets.
Set them once, in `backend/TaskFlow.Api`:

```bash
dotnet user-secrets set "Seed:AdministratorEmail" "admin@example.com"
dotnet user-secrets set "Seed:AdministratorDisplayName" "System Admin"
dotnet user-secrets set "Seed:AdministratorPassword" "<choose a password>"
```

The two other demo accounts are made once by the admin, in **Users → New user**,
and then added to a team:

| Role         | E-mail                       | Password                       |
| ------------ | ---------------------------- | ------------------------------ |
| Admin        | the e-mail from user secrets | the password from user secrets |
| User         | `user@taskflow.test`         | `Demo1234`                     |
| ReadOnlyUser | `readonly@taskflow.test`     | `Demo1234`                     |

New accounts made with **Create account** get the **User** role. An
administrator adds them to a team.

## Scripts

| Command              | What it does                                 |
| -------------------- | -------------------------------------------- |
| `npm run dev`        | Start the app on port 5173                   |
| `npm run build`      | Build for production into `dist/`            |
| `npm run preview`    | Serve the production build                   |
| `npm run lint`       | Check the code with oxlint                   |
| `npm test`           | Run the Vitest + React Testing Library tests |
| `npm run test:watch` | Run the tests on every change                |
| `npm run storybook`  | Open Storybook on port 6006                  |

## Project structure

```
src/
  api/          API calls and mappers, one folder per resource
  auth/         token storage and permission helpers
  components/   atoms, molecules, organisms, templates
  constants/    packages, claims, features and other fixed values
  context/      current user and breadcrumbs
  hooks/        data hooks (useTeams, useProject, …)
  pages/        one folder per page
  routes/       route guards (auth, guest, package, claim, feature)
  utils/        small helpers (dates, validation, filters)
```

## Permissions

What a user sees depends on the packages, claims and features of their roles:

- **Menus, buttons and row actions** are hidden without the right permission.
- **Guarded URLs** typed by hand redirect to `/dashboard`.
- **Without a session**, every protected page redirects to `/sign-in`.
- A change to a role shows in the UI after a reload.

## Known limitations

- Writes return **202 Accepted** and are processed in the background. The app
  shows a success toast after the 202 and reloads shortly after. Errors the
  backend finds later (for example a rule violation or a concurrency conflict)
  are only logged by the API, because the live SignalR notifications are not
  connected yet.

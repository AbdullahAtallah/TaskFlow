# Getting started

## 1. Run the backend

You need .NET SDK 10, SQL Server, and the EF tools (`dotnet tool update --global dotnet-ef --version 10.0.12`).

**Once:** in the `backend` folder, set the secrets (see [README.md](README.md#configuration)), then create the
database:

```powershell
dotnet ef database update --project TaskFlow.Infrastructure --startup-project TaskFlow.Api
```

**Every time:**

```powershell
dotnet run --project TaskFlow.Api
```

The API runs at `http://localhost:5080`. Stop it with **Ctrl+C**.

## 2. Swagger

Swagger is a web page where you can try every endpoint. Open **http://localhost:5080/swagger**.

**Sign in:**

1. `POST /api/auth/login` → **Try it out** → enter your e-mail and password → **Execute**.
2. Copy the `accessToken` from the response.
3. Click **Authorize** (top right), paste the token (without "Bearer") and confirm.

**What's in it:** the endpoints grouped per area (Auth, Team, Project, Task, User, Role, …). `…Query` groups are the
reads.

**Good to know:**

- Delete `userCommandContext` and `correlationId` from example bodies: the server fills them in.
- Most writes answer `202` with no body. To see the result, call the matching GET.

## 3. Token and JWT

A **token** is like a wristband at a festival: you show your password once (login), get the wristband, and show only
the wristband after that. It is valid for **8 hours**.

A **JWT** is that token: one text in three parts, `header.payload.signature`.

- The **payload** says who you are: user id (`sub`), `email`, `name`, and the expiry time (`exp`).
- The **signature** proves the API made it. Nobody can change it without the secret `Jwt:SigningKey`.
- Anyone can read a JWT (try [jwt.io](https://jwt.io)), so never share yours.

**How it works:** the frontend sends `Authorization: Bearer <token>` with every request. The API checks the signature
and the expiry. Then it reads your permissions from the database, so the token itself holds no permissions.

| Answer  | Means                                                                   |
|---------|-------------------------------------------------------------------------|
| **401** | No token, or it is wrong, expired, logged out or deactivated: sign in again |
| **403** | Signed in, but missing the permission                                   |

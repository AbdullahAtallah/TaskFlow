import client from "../client";

export const login = ({ email, password }) =>
  client.post("/auth/login", { email, password }).then((res) => res.data);

export const register = ({ email, displayName, password }) =>
  client
    .post("/auth/register", { email, displayName, password })
    .then((res) => res.data);

export const logout = () => client.post("/auth/logout");

import client from "../client";
import gridParams from "../gridParams";

export const getCurrentUser = () =>
  client.get("/user/current").then((res) => res.data);

export const getUsers = (options) =>
  client.get("/user", { params: gridParams(options) }).then((res) => res.data);

export const updateUser = (id, user) =>
  client.put(`/user/${id}`, { id, ...user }).then((r) => r.data);

export const createUser = (user) =>
  client.post("/user", user).then((r) => r.data);

export const setUserPassword = (id, password) =>
  client
    .put(`/user/${id}/password`, { userId: id, password })
    .then((r) => r.data);

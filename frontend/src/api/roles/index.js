import client from "../client";
import gridParams from "../gridParams";

export const getRoles = (options) =>
  client.get("/role", { params: gridParams(options) }).then((r) => r.data);

export const getRole = (id) => client.get(`/role/${id}`).then((r) => r.data);

export const getRoleUsers = (id, options) =>
  client
    .get(`/role/${id}/user`, { params: gridParams(options) })
    .then((r) => r.data);

export const updateRole = (id, { packages, claims }) =>
  client.put(`/role/${id}`, { id, packages, claims }).then((r) => r.data);

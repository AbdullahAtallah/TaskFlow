import client from "../client";
import gridParams from "../gridParams";

export const getTeams = (options) =>
  client.get("/team", { params: gridParams(options) }).then((r) => r.data);

export const getTeam = (id) => client.get(`/team/${id}`).then((r) => r.data);

export const getTeamMembers = (id) =>
  client.get(`/team/${id}/member`).then((r) => r.data);

export const createTeam = (team) => client.post("/team", team);

export const updateTeam = (id, { name, description, rowVersion }) =>
  client.put(`/team/${id}`, { id, name, description, rowVersion });

export const deleteTeam = (id) => client.delete(`/team/${id}`);

export const addTeamMember = (id, member) =>
  client.post(`/team/${id}/member`, member);

export const removeTeamMember = (id, userId) =>
  client.delete(`/team/${id}/member/${userId}`);

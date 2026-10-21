import client from "../client";
import gridParams from "../gridParams";

export const getTeams = ({ onlyMine, ...options } = {}) =>
  client
    .get("/team", {
      params: { ...gridParams(options), ...(onlyMine && { onlyMine: true }) },
    })
    .then((r) => r.data);

export const getTeam = (id) => client.get(`/team/${id}`).then((r) => r.data);

export const getTeamMembers = (id) =>
  client.get(`/team/${id}/member`).then((r) => r.data);

export const createTeam = (team) => client.post("/team", team);

export const updateTeam = (id, { name, description, rowVersion }) =>
  client.put(`/team/${id}`, { id, name, description, rowVersion });

export const deleteTeam = (id) => client.delete(`/team/${id}`);

export const addTeamMembers = (teamId, userIds) =>
  client.post(`/team/${teamId}/member`, { teamId, userIds });

export const removeTeamMember = (teamId, userId) =>
  client.delete(`/team/${teamId}/member/${userId}`);

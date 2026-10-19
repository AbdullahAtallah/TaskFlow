import client from "../client";
import gridParams from "../gridParams";

export const getProjects = ({ teamId, ...options } = {}) =>
  client
    .get("/project", {
      params: { ...gridParams(options), ...(teamId && { teamId }) },
    })
    .then((r) => r.data);

export const deleteProject = (id) => client.delete(`/project/${id}`);

export const createProject = ({ teamId, name, description }) =>
  client.post("/project", { teamId, name, description });

export const getProject = (id) =>
  client.get(`/project/${id}`).then((r) => r.data);

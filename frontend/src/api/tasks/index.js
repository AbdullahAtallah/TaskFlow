import client from "../client";
import gridParams from "../gridParams";

export const getTasks = ({ projectId, ...options } = {}) =>
  client
    .get("/task", {
      params: { ...gridParams(options), ...(projectId && { projectId }) },
    })
    .then((r) => r.data);

export const getTask = (id) => client.get(`/task/${id}`).then((r) => r.data);

export const createTask = (task) => client.post("/task", task);

export const updateTask = (id, task) =>
  client.put(`/task/${id}`, { id, ...task });

export const deleteTask = (id) => client.delete(`/task/${id}`);

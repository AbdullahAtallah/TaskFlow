import client from "../client";
import gridParams from "../gridParams";

export const getTasks = ({ projectId, ...options } = {}) =>
  client
    .get("/task", { params: { ...gridParams(options), ...(projectId && { projectId }) } })
    .then((r) => r.data);
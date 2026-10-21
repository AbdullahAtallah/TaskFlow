import client from "../client";
import gridParams from "../gridParams";

export const getMyTasks = (options) =>
  client
    .get("/task/my-tasks", { params: gridParams(options) })
    .then((r) => r.data);

export const getStatusCounts = () =>
  client.get("/task/status-counts").then((r) => r.data);

export const getProjectCountsPerTeam = () =>
  client.get("/project/counts-per-team").then((r) => r.data);

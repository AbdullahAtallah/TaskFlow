import client from "../client";
import gridParams from "../gridParams";

export const getRoles = (options) =>
  client.get("/role", { params: gridParams(options) }).then((r) => r.data);

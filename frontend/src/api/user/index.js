import client from "../client";
import gridParams from "../gridParams";

export const getCurrentUser = () =>
  client.get("/user/current").then((res) => res.data);

export const getUsers = (options) =>
  client.get("/user", { params: gridParams(options) }).then((res) => res.data);

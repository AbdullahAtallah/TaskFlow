import client from "../client";

export const getCurrentUser = () =>
  client.get("/user/current").then((res) => res.data);

import { createPermissions } from "./permissions";

const user = {
  packages: ["General", "Teams"],
  features: ["Tasks.Board"],
  claims: ["Teams.Manage", "Tasks.Create"],
};

describe("createPermissions", () => {
  it("checks packages", () => {
    const p = createPermissions(user);
    expect(p.hasPackage("Teams")).toBe(true);
    expect(p.hasPackage("Management")).toBe(false);
  });

  it("checks features", () => {
    const p = createPermissions(user);
    expect(p.hasFeature("Tasks.Board")).toBe(true);
    expect(p.hasFeature("Other")).toBe(false);
  });

  it("checks claims", () => {
    const p = createPermissions(user);
    expect(p.hasClaim("Teams.Manage")).toBe(true);
    expect(p.hasClaim("Projects.Create")).toBe(false);
  });

  it("checks any of several claims", () => {
    const p = createPermissions(user);
    expect(p.hasAnyClaim("Projects.Create", "Tasks.Create")).toBe(true);
    expect(p.hasAnyClaim("Projects.Create", "Users.Manage")).toBe(false);
  });

  it("denies everything without a user", () => {
    const p = createPermissions(null);
    expect(p.hasPackage("General")).toBe(false);
    expect(p.hasClaim("Teams.Manage")).toBe(false);
    expect(p.hasAnyClaim("Teams.Manage")).toBe(false);
  });
});

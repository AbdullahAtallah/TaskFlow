export const PERMISSION_CATALOG = [
  {
    name: "General",
    description: "Dashboard and profile",
    alwaysOn: true,
    claims: [],
  },
  {
    name: "Teams",
    description: "Teams and their members",
    claims: [
      {
        name: "Teams.View.All",
        description: "See every team, not only the teams you are a member of",
      },
      { name: "Teams.Manage", description: "Create, edit and delete teams" },
      {
        name: "Teams.Members.Manage",
        description: "Add and remove team members",
      },
    ],
  },
  {
    name: "Projects",
    description: "Projects owned by teams",
    claims: [
      {
        name: "Projects.View.All",
        description: "See the projects of every team",
      },
      {
        name: "Projects.Create",
        description: "Create a project in a team inside your scope",
      },
      { name: "Projects.Manage", description: "Edit and delete projects" },
    ],
  },
  {
    name: "Tasks",
    description: "Tasks inside projects",
    claims: [
      { name: "Tasks.View.All", description: "See the tasks of every project" },
      { name: "Tasks.Create", description: "Create tasks" },
      {
        name: "Tasks.Manage",
        description: "Edit tasks, change their status and delete them",
      },
      { name: "Tasks.Assign", description: "Assign a task to a team member" },
      {
        name: "Tasks.Board.View",
        description: "Open the task board of a project",
        feature: "Tasks.Board",
      },
    ],
  },
  {
    name: "Management",
    description: "Administration of users and roles",
    claims: [],
  },
];

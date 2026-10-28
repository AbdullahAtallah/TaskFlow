import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import PackageCard from "./PackageCard";

const teams = {
  name: "Teams",
  description: "Teams and their members",
  claims: [
    { name: "Teams.View.All", description: "See every team" },
    { name: "Teams.Manage", description: "Create, edit and delete teams" },
  ],
};

const renderCard = (props = {}) => {
  const onTogglePackage = vi.fn();
  const onToggleClaim = vi.fn();
  render(
    <PackageCard
      pkg={teams}
      enabled
      claims={["Teams.Manage"]}
      onTogglePackage={onTogglePackage}
      onToggleClaim={onToggleClaim}
      {...props}
    />,
  );
  return { onTogglePackage, onToggleClaim };
};

describe("PackageCard", () => {
  it("shows the claims with their state", () => {
    renderCard();
    expect(screen.getByLabelText("Teams.Manage")).toBeChecked();
    expect(screen.getByLabelText("Teams.View.All")).not.toBeChecked();
  });

  it("turns the package off", async () => {
    const { onTogglePackage } = renderCard();
    await userEvent.click(screen.getByLabelText("Teams"));
    expect(onTogglePackage).toHaveBeenCalledWith(teams, false);
  });

  it("grants a claim", async () => {
    const { onToggleClaim } = renderCard();
    await userEvent.click(screen.getByLabelText("Teams.View.All"));
    expect(onToggleClaim).toHaveBeenCalledWith("Teams.View.All", true);
  });

  it("disables the claims when the package is off", () => {
    renderCard({ enabled: false });
    expect(screen.getByLabelText("Teams.Manage")).toBeDisabled();
  });

  it("shows the lock note", () => {
    renderCard({ locked: true, lockedNote: "You can't remove this." });
    expect(screen.getByLabelText("Teams")).toBeDisabled();
    expect(screen.getByText("You can't remove this.")).toBeInTheDocument();
  });
});

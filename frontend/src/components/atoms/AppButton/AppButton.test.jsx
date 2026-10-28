import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import AppButton from "./AppButton";

describe("AppButton", () => {
  it("shows its text and handles a click", async () => {
    const onClick = vi.fn();
    render(<AppButton onClick={onClick}>Save</AppButton>);

    await userEvent.click(screen.getByRole("button", { name: "Save" }));

    expect(onClick).toHaveBeenCalledTimes(1);
  });

  it("is disabled and cannot be clicked", () => {
    const onClick = vi.fn();
    render(
      <AppButton onClick={onClick} disabled>
        Save
      </AppButton>,
    );
    const button = screen.getByRole("button", { name: "Save" });
    expect(button).toBeDisabled();
    expect(button).toHaveStyle({ pointerEvents: "none" });
    expect(onClick).not.toHaveBeenCalled();
  });
});

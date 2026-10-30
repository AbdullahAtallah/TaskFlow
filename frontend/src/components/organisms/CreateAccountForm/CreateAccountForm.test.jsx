import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router";
import CreateAccountForm from "./CreateAccountForm";
import { register } from "../../../api/auth";
import { getEmailError, getPasswordError } from "../../../utils/validation";

vi.mock("../../../api/auth", () => ({ register: vi.fn() }));
vi.mock("../../../auth/tokenStorage", () => ({ saveToken: vi.fn() }));
vi.mock("notistack", () => ({
  useSnackbar: () => ({ enqueueSnackbar: vi.fn() }),
}));
vi.mock("../../../context/CurrentUserContext", () => ({
  useCurrentUser: () => ({ reloadUser: vi.fn() }),
}));

const renderForm = () =>
  render(
    <MemoryRouter>
      <CreateAccountForm />
    </MemoryRouter>,
  );

const submit = () =>
  userEvent.click(screen.getByRole("button", { name: "Create account" }));

const fill = async ({ name, email, password, repeat, terms = true }) => {
  if (name) await userEvent.type(screen.getByLabelText("Full name"), name);
  if (email) await userEvent.type(screen.getByLabelText("E-mail"), email);
  if (password)
    await userEvent.type(screen.getByLabelText("Password"), password);
  if (repeat)
    await userEvent.type(screen.getByLabelText("Repeat password"), repeat);
  if (terms) await userEvent.click(screen.getByRole("checkbox"));
};

beforeEach(() => vi.clearAllMocks());

describe("CreateAccountForm validation", () => {
  it("shows every error for an empty form", async () => {
    renderForm();
    await submit();

    expect(screen.getByText("Enter your full name.")).toBeInTheDocument();
    expect(screen.getByText(getEmailError(""))).toBeInTheDocument();
    expect(screen.getByText(getPasswordError(""))).toBeInTheDocument();
    expect(
      screen.getByText("Accept the terms to continue."),
    ).toBeInTheDocument();
    expect(register).not.toHaveBeenCalled();
  });

  it("rejects a weak password", async () => {
    renderForm();
    await fill({
      name: "Laura",
      email: "laura@example.com",
      password: "short",
      repeat: "short",
    });
    await submit();

    expect(screen.getByText(getPasswordError("short"))).toBeInTheDocument();
    expect(register).not.toHaveBeenCalled();
  });

  it("rejects passwords that don't match", async () => {
    renderForm();
    await fill({
      name: "Laura",
      email: "laura@example.com",
      password: "Secret123",
      repeat: "Secret124",
    });
    await submit();

    expect(screen.getByText("The passwords don't match.")).toBeInTheDocument();
    expect(register).not.toHaveBeenCalled();
  });

  it("requires the terms", async () => {
    renderForm();
    await fill({
      name: "Laura",
      email: "laura@example.com",
      password: "Secret123",
      repeat: "Secret123",
      terms: false,
    });
    await submit();

    expect(
      screen.getByText("Accept the terms to continue."),
    ).toBeInTheDocument();
    expect(register).not.toHaveBeenCalled();
  });

  it("creates the account with valid input", async () => {
    register.mockResolvedValue("token-123");
    renderForm();
    await fill({
      name: " Laura Bakker ",
      email: "laura@example.com",
      password: "Secret123",
      repeat: "Secret123",
    });
    await submit();

    expect(register).toHaveBeenCalledWith({
      email: "laura@example.com",
      displayName: "Laura Bakker",
      password: "Secret123",
    });
  });
});

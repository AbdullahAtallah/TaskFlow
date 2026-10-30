import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router";
import SignInForm from "./SignInForm";
import { login } from "../../../api/auth";
import { saveToken } from "../../../auth/tokenStorage";
import { getEmailError } from "../../../utils/validation";

vi.mock("../../../api/auth", () => ({ login: vi.fn() }));
vi.mock("../../../auth/tokenStorage", () => ({ saveToken: vi.fn() }));
vi.mock("notistack", () => ({
  useSnackbar: () => ({ enqueueSnackbar: vi.fn() }),
}));

const reloadUser = vi.fn();
vi.mock("../../../context/CurrentUserContext", () => ({
  useCurrentUser: () => ({ reloadUser }),
}));

const renderForm = () =>
  render(
    <MemoryRouter>
      <SignInForm />
    </MemoryRouter>,
  );

const submit = () =>
  userEvent.click(screen.getByRole("button", { name: "Sign in" }));

beforeEach(() => vi.clearAllMocks());

describe("SignInForm validation", () => {
  it("shows errors for empty fields and does not call the API", async () => {
    renderForm();
    await submit();

    expect(screen.getByText(getEmailError(""))).toBeInTheDocument();
    expect(screen.getByText("Enter your password.")).toBeInTheDocument();
    expect(login).not.toHaveBeenCalled();
  });

  it("rejects an invalid e-mail address", async () => {
    renderForm();
    await userEvent.type(screen.getByLabelText("E-mail"), "abc");
    await userEvent.type(screen.getByLabelText("Password"), "secret");
    await submit();

    expect(screen.getByText(getEmailError("abc"))).toBeInTheDocument();
    expect(login).not.toHaveBeenCalled();
  });

  it("signs in with valid input", async () => {
    login.mockResolvedValue("token-123");
    renderForm();
    await userEvent.type(
      screen.getByLabelText("E-mail"),
      "  sara@example.com ",
    );
    await userEvent.type(screen.getByLabelText("Password"), "secret");
    await submit();

    expect(login).toHaveBeenCalledWith({
      email: "sara@example.com",
      password: "secret",
    });
    expect(saveToken).toHaveBeenCalledWith("token-123", true);
    expect(reloadUser).toHaveBeenCalled();
  });

  it("shows a banner for a wrong password", async () => {
    login.mockRejectedValue({ response: { status: 401, data: {} } });
    renderForm();
    await userEvent.type(screen.getByLabelText("E-mail"), "sara@example.com");
    await userEvent.type(screen.getByLabelText("Password"), "wrong");
    await submit();

    expect(
      await screen.findByText("Wrong e-mail address or password."),
    ).toBeInTheDocument();
  });

  it("shows a banner when the server cannot be reached", async () => {
    login.mockRejectedValue(new Error("Network Error"));
    renderForm();
    await userEvent.type(screen.getByLabelText("E-mail"), "sara@example.com");
    await userEvent.type(screen.getByLabelText("Password"), "secret");
    await submit();

    expect(
      await screen.findByText(
        "Can't reach the server. Check that the API is running.",
      ),
    ).toBeInTheDocument();
  });
});

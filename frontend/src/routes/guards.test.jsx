import { render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router";
import { useCurrentUser } from "../context/CurrentUserContext";
import { getToken } from "../auth/tokenStorage";
import { createPermissions } from "../auth/permissions";
import AuthGuard from "./AuthGuard";
import GuestGuard from "./GuestGuard";
import PackageGuard from "./PackageGuard";
import ClaimGuard from "./ClaimGuard";
import FeatureGuard from "./FeatureGuard";

vi.mock("../context/CurrentUserContext", () => ({ useCurrentUser: vi.fn() }));
vi.mock("../auth/tokenStorage", () => ({ getToken: vi.fn() }));

const signedInAs = (user) => {
  getToken.mockReturnValue("token");
  useCurrentUser.mockReturnValue({
    user,
    loading: false,
    loadError: false,
    retryUser: vi.fn(),
    ...createPermissions(user),
  });
};

const renderGuard = (guard) =>
  render(
    <MemoryRouter initialEntries={["/private"]}>
      <Routes>
        <Route element={guard}>
          <Route path="/private" element={<p>Private page</p>} />
        </Route>
        <Route path="/sign-in" element={<p>Sign in page</p>} />
        <Route path="/dashboard" element={<p>Dashboard page</p>} />
      </Routes>
    </MemoryRouter>,
  );

const user = {
  packages: ["General", "Teams"],
  claims: ["Teams.Manage"],
  features: ["Tasks.Board"],
};

beforeEach(() => {
  vi.clearAllMocks();
});

describe("AuthGuard", () => {
  it("sends you to sign in without a token", () => {
    getToken.mockReturnValue(null);
    useCurrentUser.mockReturnValue({
      user: null,
      loading: false,
      loadError: false,
    });

    renderGuard(<AuthGuard />);

    expect(screen.getByText("Sign in page")).toBeInTheDocument();
  });

  it("shows the page with a token and a user", () => {
    signedInAs(user);
    renderGuard(<AuthGuard />);
    expect(screen.getByText("Private page")).toBeInTheDocument();
  });

  it("shows a retry when the API cannot be reached", () => {
    getToken.mockReturnValue("token");
    useCurrentUser.mockReturnValue({
      user: null,
      loading: false,
      loadError: true,
      retryUser: vi.fn(),
    });

    renderGuard(<AuthGuard />);

    expect(screen.getByText("Can't reach the server")).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: "Try again" }),
    ).toBeInTheDocument();
  });
});

describe("GuestGuard", () => {
  it("sends a signed-in user to the dashboard", () => {
    getToken.mockReturnValue("token");
    renderGuard(<GuestGuard />);
    expect(screen.getByText("Dashboard page")).toBeInTheDocument();
  });

  it("shows the page without a token", () => {
    getToken.mockReturnValue(null);
    renderGuard(<GuestGuard />);
    expect(screen.getByText("Private page")).toBeInTheDocument();
  });
});

describe("PackageGuard", () => {
  it("shows the page with the package", () => {
    signedInAs(user);
    renderGuard(<PackageGuard pkg="Teams" />);
    expect(screen.getByText("Private page")).toBeInTheDocument();
  });

  it("sends you to the dashboard without the package", () => {
    signedInAs(user);
    renderGuard(<PackageGuard pkg="Management" />);
    expect(screen.getByText("Dashboard page")).toBeInTheDocument();
  });
});

describe("ClaimGuard", () => {
  it("shows the page with the claim", () => {
    signedInAs(user);
    renderGuard(<ClaimGuard claim="Teams.Manage" />);
    expect(screen.getByText("Private page")).toBeInTheDocument();
  });

  it("sends you to the dashboard without the claim", () => {
    signedInAs(user);
    renderGuard(<ClaimGuard claim="Projects.Create" />);
    expect(screen.getByText("Dashboard page")).toBeInTheDocument();
  });
});

describe("FeatureGuard", () => {
  it("shows the page when the feature is on", () => {
    signedInAs(user);
    renderGuard(<FeatureGuard feature="Tasks.Board" />);
    expect(screen.getByText("Private page")).toBeInTheDocument();
  });

  it("sends you to the dashboard when the feature is off", () => {
    signedInAs({ ...user, features: [] });
    renderGuard(<FeatureGuard feature="Tasks.Board" />);
    expect(screen.getByText("Dashboard page")).toBeInTheDocument();
  });
});

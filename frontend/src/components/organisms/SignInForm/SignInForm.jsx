import { Box, Typography, Stack, Link } from "@mui/material";
import { Link as RouterLink, useLocation, useNavigate } from "react-router";

import TextField from "../../molecules/TextField/TextField";
import RememberMe from "../../molecules/RememberMe/RememberMe";
import AuthTitle from "../../atoms/AuthTitle/AuthTitle";
import AuthDescription from "../../atoms/AuthDescription/AuthDescription";
import FormField from "../../molecules/FormField/FormField";
import AlertBanner from "../../atoms/AlertBanner/AlertBanner";
import AppButton from "../../atoms/AppButton/AppButton";
import { useCurrentUser } from "../../../context/CurrentUserContext";

import { useState } from "react";
import { useSnackbar } from "notistack";

import { login } from "../../../api/auth";
import { saveToken } from "../../../auth/tokenStorage";
import { getEmailError } from "../../../utils/validation";
import { getFieldErrors } from "../../../utils/apiErrors";

const linkSx = {
  fontSize: "14px",
  cursor: "pointer",
  color: "#304FFE",
  fontWeight: 500,
};

const SignInForm = () => {
  const { reloadUser } = useCurrentUser();

  const navigate = useNavigate();
  const location = useLocation();
  const { enqueueSnackbar } = useSnackbar();

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [rememberMe, setRememberMe] = useState(true);
  const [errors, setErrors] = useState({});
  const [banner, setBanner] = useState("");
  const [submitting, setSubmitting] = useState(false);

  const validate = () => {
    const result = {};
    const emailError = getEmailError(email);
    if (emailError) result.email = emailError;
    if (!password) result.password = "Enter your password.";
    return result;
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    setBanner("");

    const clientErrors = validate();
    setErrors(clientErrors);
    if (Object.keys(clientErrors).length > 0) return;

    setSubmitting(true);
    try {
      const token = await login({ email: email.trim(), password });
      saveToken(token, rememberMe);
      await reloadUser();

      const destination = location.state?.from?.pathname ?? "/dashboard";
      navigate(destination, { replace: true });
    } catch (error) {
      setPassword("");
      const status = error.response?.status;
      const code = error.response?.data?.error;

      if (!error.response)
        setBanner("Can't reach the server. Check that the API is running.");
      else if (status === 429)
        setBanner("Too many attempts. Try again in 15 minutes.");
      else if (code === "account_revoked")
        setBanner("This account is deactivated. Contact an administrator.");
      else if (status === 401) setBanner("Wrong e-mail address or password.");
      else if (status === 400) setErrors(getFieldErrors(error));
      else setBanner("Something went wrong. Try again.");
    } finally {
      setSubmitting(false);
    }
  };

  const handleForgotPassword = () => {
    enqueueSnackbar("Ask an administrator to reset your password.", {
      variant: "toast",
      tone: "info",
      title: "Forgot your password?",
    });
  };

  return (
    <Box component="form" onSubmit={handleSubmit}>
      <AuthTitle>Sign in</AuthTitle>
      <AuthDescription>Sign in with your work e-mail address.</AuthDescription>

      <AlertBanner>{banner}</AlertBanner>
      <Stack spacing={1.5}>
        <FormField label="E-mail" htmlFor="email" error={errors.email}>
          <TextField
            id="email"
            type="email"
            placeholder="name@example.com"
            autoComplete="email"
            value={email}
            error={Boolean(errors.email)}
            onChange={(e) => setEmail(e.target.value)}
          />
        </FormField>

        <FormField
          label="Password"
          htmlFor="password"
          error={errors.password}
          action={
            <Link
              component="button"
              type="button"
              underline="hover"
              sx={linkSx}
              onClick={handleForgotPassword}
            >
              Forgot password?
            </Link>
          }
        >
          <TextField
            id="password"
            type="password"
            autoComplete="current-password"
            value={password}
            error={Boolean(errors.password)}
            onChange={(e) => setPassword(e.target.value)}
          />
        </FormField>

        <RememberMe
          checked={rememberMe}
          onChange={(e) => setRememberMe(e.target.checked)}
        />

        <Box
          sx={{
            display: "flex",
            textAlign: "center",
            alignItems: "center",
            justifyContent: "space-between",
            gap: "16px",
            flexWrap: "wrap",
          }}
        >
          <AppButton type="submit">
            {submitting ? "Signing in…" : "Sign in"}
          </AppButton>

          <Typography sx={{ fontSize: "14px", color: "#6b7280" }}>
            No account yet?{" "}
            <Link
              component={RouterLink}
              to="/create-account"
              underline="hover"
              sx={linkSx}
            >
              Create account
            </Link>
          </Typography>
        </Box>
      </Stack>
    </Box>
  );
};

export default SignInForm;

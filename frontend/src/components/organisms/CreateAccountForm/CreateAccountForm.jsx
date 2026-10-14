import { Box, Button, Link, Typography, Stack } from "@mui/material";
import { Link as RouterLink, useNavigate } from "react-router";

import TextField from "../../molecules/TextField/TextField";
import AcceptTerms from "../../molecules/AcceptTerms/AcceptTerms";
import AuthTitle from "../../atoms/AuthTitle/AuthTitle";
import AuthDescription from "../../atoms/AuthDescription/AuthDescription";
import AlertBanner from "../../atoms/AlertBanner/AlertBanner";
import FormField from "../../molecules/FormField/FormField";
import { useCurrentUser } from "../../../context/CurrentUserContext";

import { useState } from "react";
import { useSnackbar } from "notistack";

import { register } from "../../../api/auth";
import { saveToken } from "../../../auth/tokenStorage";
import { getEmailError, getPasswordError } from "../../../utils/validation";
import { getFieldErrors } from "../../../utils/apiErrors";

const linkSx = {
  fontSize: "14px",
  cursor: "pointer",
  color: "#304FFE",
  fontWeight: 500,
};

const CreateAccountForm = () => {
  const { reloadUser } = useCurrentUser();

  const navigate = useNavigate();
  const { enqueueSnackbar } = useSnackbar();

  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [repeatPassword, setRepeatPassword] = useState("");
  const [acceptTerms, setAcceptTerms] = useState(false);
  const [errors, setErrors] = useState({});
  const [banner, setBanner] = useState("");
  const [submitting, setSubmitting] = useState(false);

  const validate = () => {
    const result = {};
    if (!name.trim()) result.displayName = "Enter your full name.";

    const emailError = getEmailError(email);
    if (emailError) result.email = emailError;

    const passwordError = getPasswordError(password);
    if (passwordError) result.password = passwordError;

    if (repeatPassword !== password)
      result.repeatPassword = "The passwords don't match.";
    if (!acceptTerms) result.acceptTerms = "Accept the terms to continue.";
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
      const token = await register({
        email: email.trim(),
        displayName: name.trim(),
        password,
      });
      saveToken(token, false);
      await reloadUser();

      navigate("/dashboard", { replace: true });
      enqueueSnackbar("An administrator adds you to a team.", {
        variant: "toast",
        tone: "info",
        title: "You have the User role",
      });
    } catch (error) {
      const status = error.response?.status;

      if (!error.response)
        setBanner("Can't reach the server. Check that the API is running.");
      else if (status === 400) setErrors(getFieldErrors(error));
      else if (status === 429)
        setBanner("Too many attempts. Try again in 15 minutes.");
      else setBanner("Something went wrong. Try again.");
    } finally {
      setSubmitting(false);
    }
  };
  return (
    <Box component="form" onSubmit={handleSubmit}>
      <AuthTitle>Create account</AuthTitle>
      <AuthDescription>
        New accounts get the User role. An administrator adds you to a team.
      </AuthDescription>

      <AlertBanner>{banner}</AlertBanner>

      <Stack spacing={1.5}>
        <FormField
          label="Full name"
          htmlFor="displayName"
          error={errors.displayName}
        >
          <TextField
            id="displayName"
            placeholder="e.g. Laura Bakker"
            autoComplete="name"
            value={name}
            error={Boolean(errors.displayName)}
            onChange={(e) => setName(e.target.value)}
          />
        </FormField>

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
          hint="At least 8 characters, including a number"
        >
          <TextField
            id="password"
            type="password"
            autoComplete="new-password"
            value={password}
            error={Boolean(errors.password)}
            onChange={(e) => setPassword(e.target.value)}
          />
        </FormField>

        <FormField
          label="Repeat password"
          htmlFor="repeatPassword"
          error={errors.repeatPassword}
        >
          <TextField
            id="repeatPassword"
            type="password"
            autoComplete="new-password"
            value={repeatPassword}
            error={Boolean(errors.repeatPassword)}
            onChange={(e) => setRepeatPassword(e.target.value)}
          />
        </FormField>

        <Box>
          <AcceptTerms
            checked={acceptTerms}
            onChange={(event) => setAcceptTerms(event.target.checked)}
          />
          {errors.acceptTerms && (
            <Typography sx={{ color: "#dc2626", fontSize: "13px" }}>
              {errors.acceptTerms}
            </Typography>
          )}
        </Box>

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
          <Button
            type="submit"
            variant="contained"
            sx={{
              textTransform: "none",
              fontSize: "14",
              fontWeight: 500,
              height: "38px",
              p: "0px 12px",
              borderRadius: "6px",
              backgroundColor: "#365aff",
            }}
          >
            {submitting ? "Creating account…" : "Create account"}
          </Button>

          <Typography sx={{ fontSize: "14px", color: "#6b7280" }}>
            Have an account?{" "}
            <Link
              component={RouterLink}
              to="/sign-in"
              underline="hover"
              sx={linkSx}
            >
              Sign in
            </Link>
          </Typography>
        </Box>
      </Stack>
    </Box>
  );
};

export default CreateAccountForm;

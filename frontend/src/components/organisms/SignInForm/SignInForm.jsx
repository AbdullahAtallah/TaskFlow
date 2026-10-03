import { Box, Button, Link, Typography, Stack } from "@mui/material";

import EmailField from "../../molecules/EmailField/EmailField";
import PasswordField from "../../molecules/PasswordField/PasswordField";
import RememberMe from "../../molecules/RememberMe/RememberMe";
import AuthTitle from "../../atoms/AuthTitle/AuthTitle";
import AuthDescription from "../../atoms/AuthDescription/AuthDescription";
import { useState } from "react";

const SignInForm = () => {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [rememberMe, setRememberMe] = useState(true);

  const handleSubmit = (event) => {
    event.preventDefault();
  };
  return (
    <Box component="form" onSubmit={handleSubmit}>
      <AuthTitle>Sign in</AuthTitle>
      <AuthDescription>Sign in with your work e-mail address.</AuthDescription>
      <Stack spacing={1.5}>
        <Box>
          <Box>
            <Box
              sx={{
                display: "flex",
                justifyContent: "space-between",
                alignItems: "center",
                mb: 1,
              }}
            >
              <Typography
                component="label"
                sx={{
                  fontSize: "14px",
                  color: "#5a5c65",
                  fontWeight: 500,
                }}
              >
                E-mail
              </Typography>
            </Box>
            <EmailField
              value={email}
              onChange={(event) => setEmail(event.target.value)}
            />
          </Box>
        </Box>

        <Box>
          <Box
            sx={{
              display: "flex",
              justifyContent: "space-between",
              alignItems: "center",
              mb: 1,
            }}
          >
            <Typography
              component="label"
              sx={{ fontSize: "14px", color: "#5a5c65", fontWeight: 500 }}
            >
              Password
            </Typography>
            <Link
              href="#"
              underline="hover"
              sx={{
                fontSize: "14px",
                cursor: "pointer",
                color: "#365aff",
                fontWeight: 500,
              }}
            >
              Forgot password?
            </Link>
          </Box>
          <PasswordField
            value={password}
            onChange={(event) => setPassword(event.target.value)}
          />
        </Box>
        <Box>
          <RememberMe
            checked={rememberMe}
            onChange={(event) => setRememberMe(event.target.checked)}
          />
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
            Sign in
          </Button>

          <Typography sx={{ fontSize: "14px", color: "#6b7280" }}>
            No account yet?{" "}
            <Link
              href="#"
              underline="hover"
              sx={{
                fontSize: "14px",
                cursor: "pointer",
                color: "#304FFE",
                fontWeight: 500,
              }}
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

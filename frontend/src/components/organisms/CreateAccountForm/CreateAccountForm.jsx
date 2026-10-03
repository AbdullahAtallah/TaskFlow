import { Box, Button, Link, Typography, Stack } from "@mui/material";

import EmailField from "../../molecules/EmailField/EmailField";
import PasswordField from "../../molecules/PasswordField/PasswordField";
import TextField from "../../molecules/TextField/TextField";
import AcceptTerms from "../../molecules/AcceptTerms/AcceptTerms";
import AuthTitle from "../../atoms/AuthTitle/AuthTitle";
import AuthDescription from "../../atoms/AuthDescription/AuthDescription";
import { useState } from "react";

const CreateAccountForm = () => {
  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [acceptTerms, setAcceptTerms] = useState(false);

  const handleSubmit = (event) => {
    event.preventDefault();
  };
  return (
    <Box component="form" onSubmit={handleSubmit}>
      <AuthTitle>Create account</AuthTitle>
      <AuthDescription>
        New accounts get the User role. An administrator adds you to a team.
      </AuthDescription>
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
                Full name
              </Typography>
            </Box>
            <TextField
              value={name}
              onChange={(event) => setName(event.target.value)}
            />
          </Box>
        </Box>

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
          </Box>
          <PasswordField
            value={password}
            onChange={(event) => setPassword(event.target.value)}
          />
          <Typography sx={{ color: "#9a8f8f", fontSize: 14, mt: 1 }}>
            At least 8 characters, including a number
          </Typography>
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
              Repeat password
            </Typography>
          </Box>
          <PasswordField
            value={password}
            onChange={(event) => setPassword(event.target.value)}
          />
        </Box>
        <Box>
          <AcceptTerms
            checked={acceptTerms}
            onChange={(event) => setAcceptTerms(event.target.checked)}
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
            Create account
          </Button>

          <Typography sx={{ fontSize: "14px", color: "#6b7280" }}>
            Have an account?{" "}
            <Link
              href="/sign-in"
              underline="hover"
              sx={{
                fontSize: "14px",
                cursor: "pointer",
                color: "#304FFE",
                fontWeight: 500,
              }}
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

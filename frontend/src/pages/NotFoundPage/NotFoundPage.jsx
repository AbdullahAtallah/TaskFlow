import { Link, Typography } from "@mui/material";
import { Link as RouterLink } from "react-router";
import AuthLayout from "../../components/templates/AuthLayout/AuthLayout";
import AuthTitle from "../../components/atoms/AuthTitle/AuthTitle";
import AuthDescription from "../../components/atoms/AuthDescription/AuthDescription";

const NotFoundPage = () => {
  return (
    <AuthLayout>
      <div>
        <AuthTitle>Page not found</AuthTitle>
        <AuthDescription>
          The page you're looking for doesn't exist or has moved.
        </AuthDescription>
        <Typography sx={{ fontSize: "14px" }}>
          <Link
            component={RouterLink}
            to="/dashboard"
            underline="hover"
            sx={{ color: "#365aff", fontWeight: 500 }}
          >
            Back to the dashboard
          </Link>
        </Typography>
      </div>
    </AuthLayout>
  );
};

export default NotFoundPage;

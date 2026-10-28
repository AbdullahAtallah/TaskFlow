import { Link as RouterLink } from "react-router";
import AuthLayout from "../../components/templates/AuthLayout/AuthLayout";
import AuthTitle from "../../components/atoms/AuthTitle/AuthTitle";
import AuthDescription from "../../components/atoms/AuthDescription/AuthDescription";
import TextLink from "../../components/atoms/TextLink/TextLink";

const NotFoundPage = () => {
  return (
    <AuthLayout>
      <div>
        <AuthTitle>Page not found</AuthTitle>
        <AuthDescription>
          The page you're looking for doesn't exist or has moved.
        </AuthDescription>
        <TextLink component={RouterLink} to="/dashboard">
          Back to the dashboard
        </TextLink>
      </div>
    </AuthLayout>
  );
};

export default NotFoundPage;

import AuthLayout from "../../components/templates/AuthLayout/AuthLayout";
import SignInForm from "../../components/organisms/SignInForm/SignInForm";
import { GlobalStyles } from "@mui/material";

const SignInPage = () => {
  return (
    <AuthLayout>
      <GlobalStyles styles={{ body: { margin: 0, overflow: "hidden" } }} />
      <SignInForm />
    </AuthLayout>
  );
};

export default SignInPage;

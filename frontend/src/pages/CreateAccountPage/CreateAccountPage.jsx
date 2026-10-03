import AuthLayout from "../../components/templates/AuthLayout/AuthLayout";
import CreateAccountForm from "../../components/organisms/SignInForm/CreateAccountForm";

const SignInPage = () => {
  return (
    <AuthLayout>
      <CreateAccountForm />
    </AuthLayout>
  );
};

export default SignInPage;

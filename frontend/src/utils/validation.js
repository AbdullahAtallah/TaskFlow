const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

export const isValidEmail = (value) => EMAIL_PATTERN.test(value.trim());

export const getEmailError = (value) => {
  if (!value.trim()) return "Enter your e-mail address.";
  if (!isValidEmail(value)) return "Enter a valid e-mail address.";
  return null;
};

export const getPasswordError = (value) => {
  if (!value) return "Enter a password.";
  if (value.length < 8 || value.length > 128) return "Use 8 to 128 characters.";
  if (!/[A-Za-z]/.test(value) || !/\d/.test(value)) {
    return "Use at least one letter and one digit.";
  }
  return null;
};

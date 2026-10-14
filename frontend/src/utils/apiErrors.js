export const getFieldErrors = (error) => {
  const errors = error?.response?.data?.errors ?? {};
  const result = {};

  Object.entries(errors).forEach(([key, messages]) => {
    const field = key.charAt(0).toLowerCase() + key.slice(1);
    result[field] = Array.isArray(messages) ? messages[0] : messages;
  });

  if (result.userName && !result.email) result.email = result.userName;

  return result;
};

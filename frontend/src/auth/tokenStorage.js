const KEY = "taskflow.token";

export const saveToken = ({ accessToken, expiresAt }, remember) => {
  clearToken();
  const storage = remember ? localStorage : sessionStorage;
  storage.setItem(KEY, JSON.stringify({ accessToken, expiresAt }));
};

const readStored = () => {
  const raw = localStorage.getItem(KEY) ?? sessionStorage.getItem(KEY);
  if (!raw) return null;
  try {
    return JSON.parse(raw);
  } catch {
    return null;
  }
};

export const getToken = () => {
  const stored = readStored();
  if (!stored?.accessToken) return null;

  if (stored.expiresAt && new Date(stored.expiresAt) <= new Date()) {
    clearToken();
    return null;
  }
  return stored.accessToken;
};

export const clearToken = () => {
  localStorage.removeItem(KEY);
  sessionStorage.removeItem(KEY);
};

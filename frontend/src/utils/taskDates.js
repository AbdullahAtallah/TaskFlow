const startOfDay = (value) => {
  const d = new Date(value);
  d.setHours(0, 0, 0, 0);
  return d;
};

export const daysOverdue = (dueDate, status) => {
  if (!dueDate || status === "Done") return 0;
  const diff = startOfDay(new Date()) - startOfDay(dueDate);
  return Math.max(0, Math.round(diff / 86400000));
};

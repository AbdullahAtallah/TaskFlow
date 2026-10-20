const MONTHS = {
  jan: 1,
  feb: 2,
  mrt: 3,
  maa: 3,
  mar: 3,
  apr: 4,
  mei: 5,
  may: 5,
  jun: 6,
  jul: 7,
  aug: 8,
  sep: 9,
  okt: 10,
  oct: 10,
  nov: 11,
  dec: 12,
};

const pad = (n) => String(n).padStart(2, "0");

const toIso = (day, month, year) => {
  const d = new Date(year, month - 1, day);
  if (
    d.getFullYear() !== year ||
    d.getMonth() !== month - 1 ||
    d.getDate() !== day
  )
    return null;
  return `${year}-${pad(month)}-${pad(day)}`;
};

export const parseDate = (text) => {
  const value = text.trim().toLowerCase();
  if (!value) return null;

  let m = value.match(/^(\d{1,2})\s+([a-z]+)\.?\s+(\d{4})$/); // 14 okt 2026
  if (m) {
    const month = MONTHS[m[2].slice(0, 3)];
    return month ? toIso(Number(m[1]), month, Number(m[3])) : null;
  }

  m = value.match(/^(\d{1,2})[-/.](\d{1,2})[-/.](\d{4})$/); // 14-10-2026
  if (m) return toIso(Number(m[1]), Number(m[2]), Number(m[3]));

  m = value.match(/^(\d{4})-(\d{1,2})-(\d{1,2})$/); // 2026-10-14
  if (m) return toIso(Number(m[3]), Number(m[2]), Number(m[1]));

  return null;
};

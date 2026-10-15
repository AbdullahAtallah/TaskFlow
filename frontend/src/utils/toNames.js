const toNames = (list) =>
  (list ?? []).map((item) =>
    typeof item === "string" ? item : (item?.name ?? item?.key ?? ""),
  );

export default toNames;

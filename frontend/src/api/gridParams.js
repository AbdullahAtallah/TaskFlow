const gridParams = ({ skip = 0, take = 50, query = "", sort } = {}) => {
  const params = { skip, take };
  if (query) params.query = query;
  if (sort?.field) {
    params["orderByFields[0].field"] = sort.field;
    params["orderByFields[0].dir"] = sort.dir ?? "asc";
  }
  return params;
};

export default gridParams;
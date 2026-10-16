const afterWrite = (reload, delay = 600) =>
  new Promise((resolve) => setTimeout(() => resolve(reload()), delay));

export default afterWrite;

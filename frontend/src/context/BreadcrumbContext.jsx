import { createContext, useContext, useEffect, useState } from "react";

const BreadcrumbContext = createContext({ crumbs: null, setCrumbs: () => {} });

export const BreadcrumbProvider = ({ children }) => {
  const [crumbs, setCrumbs] = useState(null);
  return (
    <BreadcrumbContext.Provider value={{ crumbs, setCrumbs }}>
      {children}
    </BreadcrumbContext.Provider>
  );
};

export const useBreadcrumb = () => useContext(BreadcrumbContext);

export const useBreadcrumbs = (crumbs) => {
  const { setCrumbs } = useBreadcrumb();
  const key = JSON.stringify(crumbs);

  useEffect(() => {
    setCrumbs(crumbs);
    return () => setCrumbs(null);
  }, [key, setCrumbs]);
};

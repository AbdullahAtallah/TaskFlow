export const createPermissions = (user) => {
  const packages = user?.packages ?? [];
  const features = user?.features ?? [];
  const claims = user?.claims ?? [];

  return {
    hasPackage: (pkg) => packages.includes(pkg),
    hasFeature: (feature) => features.includes(feature),
    hasClaim: (claim) => claims.includes(claim),
    hasAnyClaim: (...list) => list.some((claim) => claims.includes(claim)),
  };
};

import { Link } from "@mui/material";

const TextLink = ({ children, onClick, component, to }) => {
  return (
    <Link
      component={component}
      onClick={onClick}
      to={to}
      sx={{
        fontSize: "14px",
        fontWeight: 500,
        lineHeight: "20px",
        color: "#365aff",
        cursor: "pointer",
        textDecoration: "none",
      }}
    >
      {children}
    </Link>
  );
};

export default TextLink;

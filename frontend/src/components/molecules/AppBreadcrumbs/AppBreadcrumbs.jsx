import { Breadcrumbs, Typography } from "@mui/material";
import { ChevronRight } from "lucide-react";

const AppBreadcrumbs = ({ items }) => {
  return (
    <Breadcrumbs separator={<ChevronRight size={16} color="#6b7280" />}>
      {items.map((item, i) => {
        const isLast = i === items.length - 1;
        return (
          <Typography
            key={item}
            sx={{
              fontSize: "15px",
              fontWeight: 500,
              color: isLast ? "#1f2937" : "#6b7280",
            }}
          >
            {item}
          </Typography>
        );
      })}
    </Breadcrumbs>
  );
};

export default AppBreadcrumbs;

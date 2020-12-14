import React from "react";
import Container from "@material-ui/core/Container";

const AdminLayout = ({ children }: { children: React.ReactChild }) => {
  return (
    <div>
      <div style={{ marginBottom: "10px" }}></div>
      <Container maxWidth="lg">{children}</Container>
    </div>
  );
};

export default AdminLayout;

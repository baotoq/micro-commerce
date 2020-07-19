import React, { useEffect } from "react";
import { useDispatch } from "react-redux";

import Container from "@material-ui/core/Container";

import { fetchCategoriesAsync } from "../../store/slices/category-slice";

import CategoriesTabs from "../CategoriesTabs";

const DefaultLayout = ({ children }: { children: React.ReactChild }) => {
  const dispatch = useDispatch();

  useEffect(() => {
    dispatch(fetchCategoriesAsync());
  }, []);

  return (
    <div>
      <CategoriesTabs />
      <div style={{ marginBottom: "10px" }}></div>
      <Container maxWidth="lg">{children}</Container>
    </div>
  );
};

export default DefaultLayout;

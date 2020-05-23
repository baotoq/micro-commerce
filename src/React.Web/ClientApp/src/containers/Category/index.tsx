import React from "react";
import { useHistory, useParams } from "react-router-dom";

const Category = () => {
  const { id } = useParams<{ id: string }>();

  return <div>Category {id}</div>;
};

export default Category;

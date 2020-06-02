import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";

import Grid from "@material-ui/core/Grid";

import Product from "./Product";

import categoryService, { CategoryResponse } from "../../services/category-service";

const Category = () => {
  const { id } = useParams<{ id: string }>();
  const [category, setCategory] = useState<CategoryResponse>();

  useEffect(() => {
    const fetchCategory = async () => {
      var response = await categoryService.findAsync(+id);
      setCategory(response);
    };

    fetchCategory();
  }, [id, setCategory]);

  return (
    <div>
      {category && (
        <div>
          <Grid container spacing={2}>
            {category.products.map((product) => (
              <Grid key={product.id} item>
                <Product product={product} />
              </Grid>
            ))}
          </Grid>
        </div>
      )}
    </div>
  );
};

export default Category;

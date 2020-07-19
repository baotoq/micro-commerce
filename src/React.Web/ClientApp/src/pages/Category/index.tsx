import React, { useEffect } from "react";
import { useDispatch } from "react-redux";
import { useParams } from "react-router-dom";
import { push } from "connected-react-router";
import Grid from "@material-ui/core/Grid";
import Pagination from "@material-ui/lab/Pagination";

import ProductCard from "./ProductCard";

import categoryService, { ProductResponse } from "../../services/category-service";
import { OffsetPaged } from "../../models";

import { changeActiveTab } from "../../store/slices/category-slice";

import { useAsync, usePrevious } from "../../hooks";

const Category = () => {
  const pageSize = 10;

  const { id, page } = useParams<{ id: string; page: string }>();
  const previousId = usePrevious(id);

  const dispatch = useDispatch();

  const { execute, pending, value, error } = useAsync<OffsetPaged<ProductResponse>>(
    () => categoryService.findProductsByCategoryIdAsync(+id, +page, pageSize),
    { data: [], totalPages: 0, totalCount: 0 },
    false
  );

  const handleChange = async (event: React.ChangeEvent<unknown>, value: number) =>
    dispatch(push(`/category/${id}/page/${value}`));

  useEffect(() => {
    execute();
    if (previousId !== id) {
      dispatch(changeActiveTab(+id));
    }
  }, [id, page, dispatch]);

  return (
    <div>
      <div>
        <Grid container spacing={2} direction="row" justify="center" alignItems="center">
          {value.data.map((product) => (
            <Grid key={product.id} item>
              <ProductCard product={product} />
            </Grid>
          ))}
        </Grid>
        <Pagination
          count={value.totalPages}
          page={+page}
          onChange={handleChange}
          color="primary"
          style={{ marginBottom: "10px" }}
        />
      </div>
    </div>
  );
};

export default Category;

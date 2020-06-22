import React, { useEffect, useState, useCallback } from "react";
import { useDispatch } from "react-redux";
import { useParams, useHistory } from "react-router-dom";

import Grid from "@material-ui/core/Grid";
import Pagination from "@material-ui/lab/Pagination";

import Product from "./Product";

import categoryService, { ProductResponse, OffsetPaged } from "../../services/category-service";

import { changeActiveTab } from "../../store/slices/category-slice";

const Category = () => {
  const pageSize = 10;

  const history = useHistory();
  const { id, page } = useParams<{ id: string; page: string }>();

  const dispatch = useDispatch();

  const { execute, pending, value, error } = useAsync<OffsetPaged<ProductResponse[]>>(
    () => categoryService.findProductsByCategoryIdAsync(+id, +page, pageSize),
    { data: [], totalPages: 0, totalCount: 0 },
    false
  );

  const handleChange = async (event: React.ChangeEvent<unknown>, value: number) =>
    history.push(`/category/${id}/page/${value}`);

  useEffect(() => {
    execute();
  }, [page]);

  useEffect(() => {
    execute();
    dispatch(changeActiveTab(+id));
  }, [id, dispatch]);

  if (pending) {
    return <div>Loading...</div>;
  }

  return (
    <div>
      <div>
        <Grid container spacing={2} direction="row" justify="center" alignItems="center">
          {value.data.map((product) => (
            <Grid key={product.id} item>
              <Product product={product} />
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

const useAsync = <T extends {}>(asyncFunction: () => Promise<T>, initValue: T, immediate = true) => {
  const [pending, setPending] = useState(false);
  const [value, setValue] = useState(initValue);
  const [error, setError] = useState<Error | null>(null);

  const execute = useCallback(async () => {
    setPending(true);
    setError(null);
    try {
      var response = await asyncFunction();
      setValue(response);
    } catch (error) {
      setError(error);
    } finally {
      setPending(false);
    }
  }, [asyncFunction]);

  useEffect(() => {
    if (immediate) {
      execute();
    }
  }, [execute, immediate]);

  return { execute, pending, value, error };
};

export default Category;

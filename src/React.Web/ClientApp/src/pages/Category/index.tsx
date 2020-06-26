import React, { useEffect, useState, useCallback, useRef } from "react";
import { useDispatch } from "react-redux";
import { useParams, useHistory } from "react-router-dom";

import Grid from "@material-ui/core/Grid";
import Pagination from "@material-ui/lab/Pagination";

import ProductCard from "./ProductCard";

import categoryService, { ProductResponse, OffsetPaged } from "../../services/category-service";

import { changeActiveTab } from "../../store/slices/category-slice";
import { setLoading } from "../../store/slices/app-slice";

const Category = () => {
  const pageSize = 10;

  const history = useHistory();
  const { id, page } = useParams<{ id: string; page: string }>();
  const previousId = usePrevious(id);

  const dispatch = useDispatch();

  const { execute, pending, value, error } = useAsync<OffsetPaged<ProductResponse>>(
    () => categoryService.findProductsByCategoryIdAsync(+id, +page, pageSize),
    { data: [], totalPages: 0, totalCount: 0 },
    false
  );

  const handleChange = async (event: React.ChangeEvent<unknown>, value: number) =>
    history.push(`/category/${id}/page/${value}`);

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

const usePrevious = <T extends {}>(value: T) => {
  // The ref object is a generic container whose current property is mutable ...
  // ... and can hold any value, similar to an instance property on a class
  const ref = useRef<T>();

  // Store current value in ref
  useEffect(() => {
    ref.current = value;
  }, [value]); // Only re-run if value changes

  // Return previous value (happens before update in useEffect above)
  return ref.current;
};

const useAsync = <T extends {}>(asyncFunction: () => Promise<T>, initValue: T, immediate = true) => {
  const [pending, setPending] = useState(false);
  const [value, setValue] = useState(initValue);
  const [error, setError] = useState<Error | null>(null);
  const dispatch = useDispatch();

  const execute = useCallback(async () => {
    dispatch(setLoading(true));
    setPending(true);
    setError(null);
    try {
      var response = await asyncFunction();
      setValue(response);
    } catch (error) {
      setError(error);
    } finally {
      setPending(false);
      dispatch(setLoading(false));
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

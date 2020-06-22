import React, { useEffect, useState } from "react";
import { useDispatch } from "react-redux";

import { changeActiveTab } from "../../store/slices/category-slice";

import Product from "../Category/Product";
import Grid from "@material-ui/core/Grid";
import productService, { ProductResponse } from "../../services/product-service";

const Home = () => {
  const [products, setProducts] = useState<ProductResponse[]>([]);
  const dispatch = useDispatch();

  useEffect(() => {
    const fetchProducts = async () => {
      var response = await productService.findAllAsync();
      setProducts(response);
    };
    dispatch(changeActiveTab(0));
    fetchProducts();
  }, [setProducts]);

  return (
    <div>
      <Grid container spacing={2}>
        {products.map((product) => (
          <Grid key={product.id} item>
            <Product product={product} />
          </Grid>
        ))}
      </Grid>
    </div>
  );
};

export default Home;

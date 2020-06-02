import React, { useEffect, useState } from "react";
import Product from "../Category/Product";
import Grid from "@material-ui/core/Grid";
import productService, { ProductResponse } from "../../services/product-service";

const Home = () => {
  const [products, setProducts] = useState<ProductResponse[]>([]);

  useEffect(() => {
    const fetchProducts = async () => {
      var response = await productService.findAllAsync();
      setProducts(response);
    };

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

import React, { useState, useEffect } from "react";
import { useParams } from "react-router-dom";

import Grid from "@material-ui/core/Grid";
import Button from "@material-ui/core/Button";
import Image from "material-ui-image";

import productService, { ProductResponse } from "../../services/product-service";

const Product = () => {
  const { id } = useParams<{ id: string }>();
  const [product, setProduct] = useState<ProductResponse>();

  useEffect(() => {
    const fetchProduct = async () => {
      var response = await productService.findAsync(+id);
      setProduct(response);
    };

    fetchProduct();
  }, [id, setProduct]);
  return (
    <div>
      {product && (
        <Grid container spacing={3}>
          <Grid item md={6}>
            <Image src={product.imageUri} aspectRatio={16 / 9} />
          </Grid>
          <Grid item md={6}>
            <h1>{product.name}</h1>
            <p>{product.description}</p>
            <h3>${product.price}</h3>
            <Button variant="contained" color="primary">
              Add to Cart
            </Button>
          </Grid>
        </Grid>
      )}
    </div>
  );
};

export default Product;

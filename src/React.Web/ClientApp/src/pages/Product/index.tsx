import React, { useState, useEffect } from "react";
import { useDispatch } from "react-redux";
import { useParams } from "react-router-dom";

import CustomerReviews from "./CustomerReviews";

import Grid from "@material-ui/core/Grid";
import Button from "@material-ui/core/Button";
import Image from "material-ui-image";

import Rating from "@material-ui/lab/Rating";

import ProductService, { ProductResponse } from "../../services/product-service";

import { addToCart } from "../../store/slices/cart-slice";

const Product = () => {
  const { id } = useParams<{ id: string }>();
  const dispatch = useDispatch();
  const [product, setProduct] = useState<ProductResponse>();

  useEffect(() => {
    const fetchProduct = async () => {
      var response = await ProductService.findByIdAsync(+id);
      setProduct(response);
    };

    fetchProduct();
  }, [id, setProduct]);
  return (
    <div>
      {product && (
        <div>
          <Grid container spacing={3}>
            <Grid item md={6}>
              <Image src={product.imageUri} aspectRatio={16 / 9} />
            </Grid>
            <Grid item md={6}>
              <h1>{product.name}</h1>
              <Rating name="read-only" value={product.ratingAverage} readOnly />
              <p>{product.description}</p>
              <h3>${product.price}</h3>
              <Button variant="contained" color="primary" onClick={() => dispatch(addToCart(product))}>
                Add to Cart
              </Button>
            </Grid>
          </Grid>
          <CustomerReviews productId={product.id} />
        </div>
      )}
    </div>
  );
};

export default Product;

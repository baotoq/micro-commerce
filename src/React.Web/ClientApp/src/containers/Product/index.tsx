import React, { useState, useEffect } from "react";
import { useParams } from "react-router-dom";
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
  return <div>{product && <div>Product {product.id}</div>}</div>;
};

export default Product;

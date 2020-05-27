import React, { useState, useEffect } from "react";
import { useParams } from "react-router-dom";
import { Row, Col, Image, Button } from "react-bootstrap";

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
        <Row>
          <Col md={6}>
            <Image src={product.imageUri} fluid />
          </Col>
          <Col md={6}>
            <h1>{product.name}</h1>
            <p>{product.description}</p>
            <h3>${product.price}</h3>
            <Button variant="primary" className="mr-2">
              Add to Cart
            </Button>
          </Col>
        </Row>
      )}
    </div>
  );
};

export default Product;

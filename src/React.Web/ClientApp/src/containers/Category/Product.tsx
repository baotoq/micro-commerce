import React from "react";
import { Link } from "react-router-dom";
import { Card, Button } from "react-bootstrap";

const Product = ({
  product,
}: {
  product: { id: number; name: string; price: number; description: string; imageUri: string };
}) => {
  return (
    <Card style={{ width: "18rem" }}>
      <Card.Img variant="top" src={product.imageUri} />
      <Card.Body>
        <Card.Title>{product.name}</Card.Title>
        <Card.Text>${product.price}</Card.Text>
        <Button variant="primary" className="mr-2">
          Add to Cart
        </Button>
        <Button variant="dark" as={Link} to={`/product/${product.id}`}>
          Details
        </Button>
      </Card.Body>
    </Card>
  );
};

export default Product;

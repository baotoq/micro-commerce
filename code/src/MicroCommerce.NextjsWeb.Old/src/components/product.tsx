"use client";
import React from "react";
import Card from "@mui/material/Card";
import CardContent from "@mui/material/CardContent";
import CardMedia from "@mui/material/CardMedia";
import Typography from "@mui/material/Typography";
import { Button, CardActionArea, CardActions, Container } from "@mui/material";
import { useShoppingCartActions } from "@/lib/store";

export default function Product({ product }: Readonly<{ product: any }>) {
  const { addProductToCart, removeProductFromCart } = useShoppingCartActions();

  return (
    <Card sx={{ maxWidth: 345 }}>
      <CardActionArea onClick={() => addProductToCart(product)}>
        <CardMedia component="img" height="140" image={product.imageUrl} alt={product.name} />
        <CardContent>
          <Typography gutterBottom variant="h5" component="div">
            {product.name}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            ${product.price.toFixed(2)}
          </Typography>
        </CardContent>
      </CardActionArea>
      <CardActions>
        <Button size="small" color="primary" onClick={() => removeProductFromCart(product)}>
          Share
        </Button>
      </CardActions>
    </Card>
  );
}

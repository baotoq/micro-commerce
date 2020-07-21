import React from "react";
import { useDispatch } from "react-redux";
import { Link } from "react-router-dom";

import { makeStyles } from "@material-ui/core/styles";
import Card from "@material-ui/core/Card";
import CardActionArea from "@material-ui/core/CardActionArea";
import CardActions from "@material-ui/core/CardActions";
import CardContent from "@material-ui/core/CardContent";
import Button from "@material-ui/core/Button";
import Typography from "@material-ui/core/Typography";
import Image from "material-ui-image";

import { addToCart } from "../../store/slices/cart-slice";

const useStyles = makeStyles({
  root: {
    maxWidth: 210,
    minWidth: 210,
  },
  media: {
    height: 110,
  },
});

interface ProductCardProps {
  product: { id: number; name: string; price: number; description: string; imageUri: string, ratingAverage: number };
}

const ProductCard = ({ product }: ProductCardProps) => {
  const classes = useStyles();
  const dispatch = useDispatch();

  return (
    <Card className={classes.root}>
      <CardActionArea component={Link} to={`/product/${product.id}`}>
        <Image src={product.imageUri} aspectRatio={16 / 9} />
        <CardContent>
          <Typography gutterBottom variant="h5" component="h2">
            {product.name}
          </Typography>
          <Typography variant="body2" color="error" component="p">
            ${product.price}
          </Typography>
        </CardContent>
      </CardActionArea>
      <CardActions>
        <Button size="small" color="primary" onClick={() => dispatch(addToCart(product))}>
          Add to Cart
        </Button>
      </CardActions>
    </Card>
  );
};

export default ProductCard;

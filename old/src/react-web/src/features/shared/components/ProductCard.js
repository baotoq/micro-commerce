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

const useStyles = makeStyles({
  root: {
    maxWidth: 210,
    minWidth: 210,
  },
  media: {
    height: 110,
  },
});

export const ProductCard = ({ product }) => {
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
        <Button
          size="small"
          color="primary"
          onClick={() => console.log("Add to cart")}
        >
          Add to Cart
        </Button>
      </CardActions>
    </Card>
  );
};

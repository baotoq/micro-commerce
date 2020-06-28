import React from "react";
import { useSelector } from "react-redux";

import { selectIsAuthenticated } from "../../store/slices/auth-slice";

import Rating from "@material-ui/lab/Rating";
import TextField from "@material-ui/core/TextField";
import Grid from "@material-ui/core/Grid";
import Button from "@material-ui/core/Button";

const CustomerReviews = () => {
  const [rating, setRating] = React.useState<number | null>(0);
  const isAuthenticated = useSelector(selectIsAuthenticated);

  return (
    <div>
      <h2>Customer reviews</h2>
      {isAuthenticated ? (
        <div>
          <Grid container spacing={1}>
            <Grid item xs={12}>
              <Rating
                value={rating}
                onChange={(event, newValue) => {
                  setRating(newValue);
                }}
              />
            </Grid>
            <Grid item xs={12}>
              <TextField fullWidth label="Title" variant="outlined" size="small" />
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                multiline
                rows={4}
                defaultValue=""
                label="Your review"
                variant="outlined"
                size="small"
              />
            </Grid>
            <Grid item xs={12}>
              <Button variant="contained" color="primary">
                Submit review
              </Button>
            </Grid>
          </Grid>
        </div>
      ) : (
        <div>
          Please login or register to submit your review. Please also note that submitting review is only enable for
          users who have bought this product
        </div>
      )}
    </div>
  );
};

export default CustomerReviews;

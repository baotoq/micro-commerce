import React from "react";
import { useSelector } from "react-redux";

import { selectIsAuthenticated } from "../../store/slices/auth-slice";
import ReviewService, { ReviewResponse } from "../../services/review-service";
import { Review, ReviewStatus, CursorPaged } from "../../models";

import Rating from "@material-ui/lab/Rating";
import TextField from "@material-ui/core/TextField";
import Grid from "@material-ui/core/Grid";
import Button from "@material-ui/core/Button";
import Snackbar from "@material-ui/core/Snackbar";
import Alert from "@material-ui/lab/Alert";

export const useStringInput = (initialValue: string) => {
  const [value, setValue] = React.useState(initialValue);

  return {
    value,
    setValue,
    reset: () => setValue(""),
    bind: {
      value,
      onChange: (event: React.ChangeEvent<HTMLInputElement>) => {
        setValue(event.target.value);
      },
    },
  };
};

const CustomerReviews = ({ productId }: { productId: number }) => {
  const [rating, setRating] = React.useState<number>(0);
  const { value: title, bind: bindTitle } = useStringInput("");
  const { value: comment, bind: bindComment } = useStringInput("");
  const isAuthenticated = useSelector(selectIsAuthenticated);

  const [reviews, setReviews] = React.useState<CursorPaged<ReviewResponse, Date>>();
  const [pageToken, setPageToken] = React.useState<Date>(new Date(Date.now()));

  const [open, setOpen] = React.useState(false);

  const handleClose = (event?: React.SyntheticEvent, reason?: string) => {
    if (reason === "clickaway") {
      return;
    }

    setOpen(false);
  };

  React.useEffect(() => {
    const fetch = async () => {
      setReviews(
        await ReviewService.findCursorAsync({ productId, reviewStatus: ReviewStatus.Approved, pageSize: 5, pageToken })
      );
    };
    fetch();
  }, [pageToken]);

  return (
    <div>
      <Snackbar open={open} autoHideDuration={5000} onClose={handleClose}>
        <Alert severity="success">Create review success, please wait for approval!</Alert>
      </Snackbar>
      <h2>Customer reviews</h2>
      {isAuthenticated ? (
        <div>
          <Grid container spacing={1}>
            <Grid item xs={12}>
              <Rating value={rating} onChange={(event, newValue) => setRating(newValue ?? 0)} name="rating" />
            </Grid>
            <Grid item xs={12}>
              <TextField fullWidth label="Title" variant="outlined" size="small" {...bindTitle} name="title" />
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                multiline
                rows={4}
                label="Your review"
                variant="outlined"
                size="small"
                {...bindComment}
                name="comment"
              />
            </Grid>
            <Grid item xs={12}>
              <Button
                variant="contained"
                color="primary"
                onClick={async () =>
                  await ReviewService.createAsync({ comment, title, rating, productId }).then(() => setOpen(true))
                }
              >
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
      <Grid container spacing={3}>
        <Grid item xs={12}>
          {reviews && (
            <div>
              {reviews.data.map((review) => (
                <div key={review.id}>
                  <Rating size="small" value={review.rating} readOnly />
                  <div>
                    <h3 style={{ display: "inline" }}>Title: </h3>
                    {review.title}
                  </div>
                  <div>
                    <h3 style={{ display: "inline" }}>Comment: </h3>
                    {review.comment}
                  </div>
                  <div>
                    <h3 style={{ display: "inline" }}>Review By: </h3>
                    {review.createdByUserName}
                  </div>
                  <br />
                </div>
              ))}
            </div>
          )}
        </Grid>
      </Grid>
      {reviews?.previousPageToken && <button onClick={() => setPageToken(reviews.previousPageToken)}>Previous</button>}
      {reviews?.nextPageToken && <button onClick={() => setPageToken(reviews.nextPageToken)}>Next</button>}
    </div>
  );
};

export default CustomerReviews;

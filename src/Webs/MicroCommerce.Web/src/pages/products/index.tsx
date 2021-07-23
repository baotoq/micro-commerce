import { useEffect } from "react";

import Link from "next/link";
import { GetServerSideProps, InferGetServerSidePropsType } from "next";
import { useSession, getSession } from 'next-auth/client'

import Button from "@material-ui/core/Button";
import Card from "@material-ui/core/Card";
import CardActions from "@material-ui/core/CardActions";
import CardContent from "@material-ui/core/CardContent";
import CardMedia from "@material-ui/core/CardMedia";
import Grid from "@material-ui/core/Grid";
import Typography from "@material-ui/core/Typography";
import { makeStyles } from "@material-ui/core/styles";

import Pagination from "@material-ui/lab/Pagination";
import PaginationItem from "@material-ui/lab/PaginationItem";

import axios from "axios";

const useStyles = makeStyles((theme) => ({
  card: {
    height: "100%",
    display: "flex",
    flexDirection: "column",
  },
  cardMedia: {
    paddingTop: "56.25%", // 16:9
  },
  cardContent: {
    flexGrow: 1,
  },
  pagination: {
    marginTop: theme.spacing(3),
  },
}));

interface ProductsProps {
  paginationResult: [{ id: number; name: string; description: string }];
  metadata: { totalPages: number };
  router: { page: number; pageSize: number };
}

export const getServerSideProps: GetServerSideProps<ProductsProps> = async (context) => {
  const page = +context.query.page || 1;
  const pageSize = +context.query.pageSize || 10;

  const { data } = await axios.get<ProductsProps>(
    `${process.env.MICRO_COMMERCE_GATEWAY_API_URL}/c/api/products?page=${page}&pageSize=${pageSize}`
  );

  console.log((await getSession(context)))

  return {
    props: {
      ...data,
      router: { page, pageSize },
    },
  };
};

export default function Products({
  router: { page, pageSize },
  paginationResult,
  metadata,
}: InferGetServerSidePropsType<typeof getServerSideProps>) {
  const classes = useStyles();

  return (
    <div>
      <Grid container spacing={4}>
        {paginationResult.map((p) => (
          <Grid item key={p.id} xs={6} sm={4} md={3}>
            <Card className={classes.card}>
              <CardMedia className={classes.cardMedia} image="https://source.unsplash.com/random" title="Image title" />
              <CardContent className={classes.cardContent}>
                <Typography gutterBottom variant="h5" component="h2">
                  {p.name} {p.id}
                </Typography>
                <Typography>{p.description}</Typography>
              </CardContent>
              <CardActions>
                <Button size="small" color="primary">
                  Add to cart
                </Button>
                <Button size="small" color="secondary">
                  Details
                </Button>
              </CardActions>
            </Card>
          </Grid>
        ))}
      </Grid>
      <Grid container className={classes.pagination}>
        <Grid item xs={12}>
          <Pagination
            page={page}
            count={metadata.totalPages}
            color="secondary"
            renderItem={(item) => (
              <Link href={`/products/?page=${item.page}&pageSize=${pageSize}`} passHref>
                <PaginationItem {...item} />
              </Link>
            )}
          />
        </Grid>
      </Grid>
    </div>
  );
}

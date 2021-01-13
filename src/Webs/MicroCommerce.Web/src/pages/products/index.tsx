import { GetServerSideProps, InferGetServerSidePropsType } from "next";

import { useAppDispatch } from "~/store";

import Button from "@material-ui/core/Button";
import Card from "@material-ui/core/Card";
import CardActions from "@material-ui/core/CardActions";
import CardContent from "@material-ui/core/CardContent";
import CardMedia from "@material-ui/core/CardMedia";
import Grid from "@material-ui/core/Grid";
import Typography from "@material-ui/core/Typography";
import { makeStyles } from "@material-ui/core/styles";

import axios from "axios";

const useStyles = makeStyles((theme) => ({
  icon: {
    marginRight: theme.spacing(2),
  },
  heroContent: {
    backgroundColor: theme.palette.background.paper,
    padding: theme.spacing(8, 0, 6),
  },
  heroButtons: {
    marginTop: theme.spacing(4),
  },
  cardGrid: {
    paddingTop: theme.spacing(8),
    paddingBottom: theme.spacing(8),
  },
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
  footer: {
    backgroundColor: theme.palette.background.paper,
    padding: theme.spacing(6),
  },
}));

interface ProductsProps {
  paginationResult: [{ id: number; name: string; description: string }];
  metadata: { totalPages: number };
}

export const getServerSideProps: GetServerSideProps<ProductsProps> = async (context) => {
  const { data } = await axios.get<ProductsProps>("https://localhost:16000/c/api/products");

  return {
    props: data,
  };
};

export default function Products({
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
                  {p.name}
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
    </div>
  );
}

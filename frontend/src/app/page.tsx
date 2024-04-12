import Card from "@mui/material/Card";
import CardContent from "@mui/material/CardContent";
import CardMedia from "@mui/material/CardMedia";
import Typography from "@mui/material/Typography";
import { Button, CardActionArea, CardActions, Container } from "@mui/material";

interface ICategory {
  id: string;
  name: string;
}

interface IProduct {
  id: string;
  name: string;
}

export default async function Home() {
  const res = await fetch(`http://localhost:5010/api/categories`);
  const data = await res.json().then((data) => data as ICategory[]);

  const res2 = await fetch(`http://localhost:5010/api/products`);
  const data2 = await res2.json().then((data) => data as IProduct[]);

  return (
    <div>
      <Container component="main" maxWidth="xl">
        {data?.map((c) => (
          <div key={c.id}>{c.name}</div>
        ))}
        <div className="grid grid-cols-4 gap-4">
          {data2?.map((product) => (
            <div key={product.id}>
              <Card sx={{ maxWidth: 345 }}>
                <CardActionArea>
                  <CardMedia
                    component="img"
                    height="140"
                    image="https://mui.com/static/images/cards/contemplative-reptile.jpg"
                    alt="green iguana"
                  />
                  <CardContent>
                    <Typography gutterBottom variant="h5" component="div">
                      {product.name}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      {product.name}
                    </Typography>
                  </CardContent>
                </CardActionArea>
                <CardActions>
                  <Button size="small" color="primary">
                    Share
                  </Button>
                </CardActions>
              </Card>
            </div>
          ))}
        </div>
      </Container>
    </div>
  );
}

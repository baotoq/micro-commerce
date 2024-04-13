"use client";
import Card from "@mui/material/Card";
import CardContent from "@mui/material/CardContent";
import CardMedia from "@mui/material/CardMedia";
import Typography from "@mui/material/Typography";
import { Button, CardActionArea, CardActions, Container } from "@mui/material";
import { useShoppingCartActions } from "@/lib/store";
import { useEffect, useState } from "react";
import { fetchData } from "next-auth/client/_utils";

interface ICategory {
  id: string;
  name: string;
}

interface IProduct {
  id: string;
  name: string;
}

export default function Home() {
  const [data, setData] = useState<ICategory[]>([]);
  const [data2, setData2] = useState<IProduct[]>([]);

  useEffect(() => {
    const fetchData = async () => {
      const res = await fetch(`http://localhost:5010/api/categories`);
      const datac = await res.json().then((data) => data as ICategory[]);
      setData(datac);
      const res2 = await fetch(`http://localhost:5010/api/products/es`);
      const data2c = await res2.json().then((data) => data as IProduct[]);
      setData2(data2c);
    };
    fetchData();
  }, []);

  const { addProductToCart } = useShoppingCartActions();

  return (
    <div>
      <Container component="main" maxWidth="xl">
        {data?.map((c) => (
          <div key={c.id}>{c.name}</div>
        ))}
        <div className="grid grid-cols-4 gap-4">
          {data2?.map((product) => (
            <Card sx={{ maxWidth: 345 }} key={product.id} onClick={() => addProductToCart(product)}>
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
          ))}
        </div>
      </Container>
    </div>
  );
}

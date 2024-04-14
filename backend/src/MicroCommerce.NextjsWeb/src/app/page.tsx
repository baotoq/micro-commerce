import { useEffect, useState } from "react";
import { Button, CardActionArea, CardActions, Container } from "@mui/material";
import Product from "@/components/product";

interface ICategory {
  id: string;
  name: string;
}

interface IProduct {
  id: string;
  name: string;
}

export default async function Home() {
  const api = process.env.services__apiservice__http__0;

  const res = await fetch(`${api}/api/categories`);
  const data = await res.json().then((data) => data as ICategory[]);
  const res2 = await fetch(`${api}/api/products/es`);
  const data2 = await res2.json().then((data) => data as IProduct[]);

  return (
    <div>
      <Container component="main" maxWidth="xl">
        {data?.map((c) => (
          <div key={c.id}>{c.name}</div>
        ))}
        <div className="grid grid-cols-4 gap-4">
          {data2?.map((product) => (
            <Product product={product} key={product.id} />
          ))}
        </div>
      </Container>
    </div>
  );
}

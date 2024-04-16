import { useEffect, useState } from "react";
import { Button, CardActionArea, CardActions, Container } from "@mui/material";
import Product from "@/components/product";
import { getServerSession } from "@/lib/next-auth";

interface ICategory {
  id: string;
  name: string;
}

interface IProduct {
  id: string;
  name: string;
  price: number;
  imageUrl: string;
}

const api = process.env.services__apiservice__http__0;

async function getCategories() {
  const res = await fetch(`${api}/api/categories`);
  return res.json().then((data) => data as ICategory[]);
}

async function getProducts() {
  const res = await fetch(`${api}/api/products`);
  return res.json().then((data) => data as IProduct[]);
}

export default async function Home() {
  const session = await getServerSession();

  console.log("session from home page", session);

  const categoriesData = getCategories();
  const productData = getProducts();

  const [categories, products] = await Promise.all([categoriesData, productData]);

  console.log("product", products);

  return (
    <div>
      <Container component="main" maxWidth="xl">
        {categories?.map((c) => (
          <div key={c.id}>{c.name}</div>
        ))}
        <div className="grid grid-cols-4 gap-4">
          {products?.map((product) => (
            <Product product={product} key={product.id} />
          ))}
        </div>
      </Container>
    </div>
  );
}

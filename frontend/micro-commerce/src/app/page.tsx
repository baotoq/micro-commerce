"use client";
import { Key } from "react";
import useSWR from "swr";

interface Category {
  id: string;
  name: string;
}

export default function Home() {
  const fetcher = (...args: any[]) => fetch(...args).then((res) => res.json());

  const { data, error, isLoading } = useSWR<Category[]>(
    "http://localhost:5010/api/categories",
    fetcher
  );

  if (error) return <div>failed to load</div>;
  if (isLoading) return <div>loading...</div>;

  return (
    <div>
      {data?.map((c) => (
        <div key={c.id}>{c.name}</div>
      ))}
    </div>
  );
}

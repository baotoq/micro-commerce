interface Category {
  id: string;
  name: string;
}

export default async function Home() {
  const res = await fetch(`http://localhost:5010/api/categories`);
  const data = await res.json().then((data) => data as Category[]);

  return (
    <div>
      {data?.map((c) => (
        <div key={c.id}>{c.name}</div>
      ))}
    </div>
  );
}

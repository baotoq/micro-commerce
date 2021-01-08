import Link from "next/link";

export default function Home() {
  return (
    <div>
      <Link href="/products/1">
        <a>this page!</a>
      </Link>
    </div>
  );
}

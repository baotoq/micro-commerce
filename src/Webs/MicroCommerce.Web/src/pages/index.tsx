import Link from "next/link";

export default function Home() {
  return (
    <div>
      <Link href="/products">
        <a>this page!</a>
      </Link>
    </div>
  );
}

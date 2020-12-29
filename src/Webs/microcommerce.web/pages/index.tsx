import Link from "next/link";

import Layout from "../components/layout";

export default function Home() {
  return (
    <Layout home>
      <div>
        <Link href="/products/first-product">
          <a>this page!</a>
        </Link>

        <footer>Footer</footer>
      </div>
    </Layout>
  );
}

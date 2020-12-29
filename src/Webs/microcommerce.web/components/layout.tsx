import Head from "next/head";
import Link from "next/link";

const name = "MicroCommerce";

export default function Layout({
  children,
  home,
}: {
  children: React.ReactNode;
  home?: boolean;
}) {
  return (
    <div>
      <Head>
        <title>{name}</title>
        <link rel="icon" href="/favicon.ico" />
        <meta name="description" content="MicroCommerce Web" />
      </Head>
      <header>
        {home ? (
          <>
            <h1>{name}</h1>
          </>
        ) : (
          <>
            <h2>
              <Link href="/">
                <a>{name}</a>
              </Link>
            </h2>
          </>
        )}
      </header>
      <main>{children}</main>
      {!home && (
        <div>
          <Link href="/">
            <a>← Back to home</a>
          </Link>
        </div>
      )}
    </div>
  );
}

import Link from "next/link";

export default function Navbar() {
  return (
    <nav>
      <Link href={"/"}>Home</Link>
      <Link href={"/login"}>Login</Link>
      <Link href={"/register"}>Register</Link>
    </nav>
  );
}

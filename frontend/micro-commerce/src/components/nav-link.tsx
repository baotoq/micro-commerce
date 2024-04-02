import Link from "next/link";

export default function NavLink({ href, children }: any) {
  return <Link href={href}>{children}</Link>;
}
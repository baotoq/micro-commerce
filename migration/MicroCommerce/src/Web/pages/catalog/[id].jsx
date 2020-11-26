import { useRouter } from "next/router";

const Catalog = () => {
  const router = useRouter();
  const { id } = router.query;

  return <p>Catalog: {id}</p>;
};

export default Catalog;

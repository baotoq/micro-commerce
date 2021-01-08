import Link from "next/link";

import { useAppDispatch, useSelector } from "~/store";
import { increment, selectValue } from "~/pages/products/product-slice";

export default function firstProduct() {
  const dispatch = useAppDispatch();
  const value = useSelector(selectValue);

  return (
    <div>
      Hello {value}
      <button onClick={() => dispatch(increment())}>Click</button>
    </div>
  );
}

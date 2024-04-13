import { create } from "zustand";
import { devtools } from "zustand/middleware";
import { immer } from "zustand/middleware/immer";

type State = {
  products: any[];
};

type StateAction = {
  actions: {
    addProductToCart: (_: any) => void;
  };
};

const initialState: State = {
  products: [],
};

const useShoppingCartStore = create<State & StateAction>()(
  immer(
    devtools((set) => ({
      ...initialState,
      actions: {
        addProductToCart: (product: any) =>
          set(
            (state) => {
              state.products.push(product);
            },
            false,
            { type: "addProductToCart", product }
          ),
      },
    }))
  )
);

export const useProductItemsCountSelector = () => useShoppingCartStore((state) => state.products.length);

export const useShoppingCartActions = () => useShoppingCartStore((state) => state.actions);

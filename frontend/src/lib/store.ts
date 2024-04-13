import { create } from "zustand";
import { devtools } from "zustand/middleware";
import { immer } from "zustand/middleware/immer";

interface IProductItem {
  id: string;
  name: string;
  productQuantity: number;
}

type State = {
  products: IProductItem[];
};

type StateAction = {
  actions: {
    addProductToCart: (_: any) => void;
    removeProductFromCart: (_: any) => void;
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
        addProductToCart: (productToAdd: any) =>
          set(
            (state) => {
              const product = state.products.find((p) => p.id === productToAdd.id);
              if (product) {
                product.productQuantity += 1;
              } else {
                state.products.push({ ...productToAdd, productQuantity: 1 });
              }
            },
            false,
            { type: "addProductToCart", productToAdd }
          ),
        removeProductFromCart: (productToRemove: any) =>
          set(
            (state) => {
              const product = state.products.find((p) => p.id === productToRemove.id);
              if (product) {
                product.productQuantity -= 1;
                if (product.productQuantity <= 0) {
                  state.products = state.products.filter((p) => p.id !== productToRemove.id);
                }
              }
            },
            false,
            { type: "removeProductFromCart", productToRemove }
          ),
      },
    }))
  )
);

export const useProductItemsCountSelector = () =>
  useShoppingCartStore((state) => state.products.reduce((acc, p) => acc + p.productQuantity, 0));

export const useShoppingCartActions = () => useShoppingCartStore((state) => state.actions);

import { create } from "zustand";

type ShoppingCartStore = {
  products: any[];
  actions: {
    addProductToCart: (_: any) => void;
  };
};

const useShoppingCartStore = create<ShoppingCartStore>()((set) => ({
  products: [],
  actions: {
    addProductToCart: (product: any) => set((state) => ({ products: [...state.products, product] })),
  },
}));

export const useProductItemsCountSelector = () => useShoppingCartStore((state) => state.products.length);

export const useShoppingCartActions = () => useShoppingCartStore((state) => state.actions);

import { createSlice, PayloadAction } from "@reduxjs/toolkit";
import { AppThunk, RootState } from "..";

import { Product } from "../../models/product";

interface CartItem {
  product: Product;
  quantity: number;
}

interface CartState {
  items: CartItem[];
}

const initialState: CartState = {
  items: [],
};

export const cart = createSlice({
  name: "cart",
  initialState,
  reducers: {
    addToCart: (state, { payload }: PayloadAction<Product>) => {
      const item = state.items.find((s) => s.product.id === payload.id);
      if (item) {
        item.quantity += 1;
      } else {
        state.items.push({ product: payload, quantity: 1 });
      }
    },
    removeFromCart: (state, { payload }: PayloadAction<number>) => {
      state.items.filter((s) => s.product.id === payload);
    },
    changeQuantity: (state, { payload }: PayloadAction<{ productId: number; quantity: number }>) => {
      const item = state.items.find((s) => s.product.id === payload.productId);
      if (item) {
        item.quantity += payload.quantity;
      }
    },
  },
});

export const { addToCart, removeFromCart, changeQuantity } = cart.actions;

export const selectCartItems = (state: RootState) => state.cart.items;
export const selectTotalItemsInCart = (state: RootState) => state.cart.items.map(s => s.quantity).reduce((accumulator, curr) => accumulator + curr, 0);

export default cart.reducer;

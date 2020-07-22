import { createSlice, PayloadAction } from "@reduxjs/toolkit";
import { AppThunk, RootState } from "..";

import { Product, CartItem } from "../../models";
import CartService from "../../services/cart-service";

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
    loadCart: (state, { payload }: PayloadAction<CartItem[]>) => {
      state.items = payload;
    },
    addToCart: (state, { payload }: PayloadAction<{ cartItemId: number; product: Product }>) => {
      const item = state.items.find((s) => s.product.id === payload.id);
      if (item) {
        item.quantity += 1;
      } else {
        state.items.push({ id: payload.cartItemId, product: payload.product, quantity: 1 });
      }
    },
    removeFromCart: (state, { payload }: PayloadAction<number>) => {
      state.items = state.items.filter((s) => s.product.id === payload);
    },
    changeQuantity: (state, { payload }: PayloadAction<{ productId: number; quantity: number }>) => {
      const item = state.items.find((s) => s.product.id === payload.productId);
      if (item) {
        item.quantity += payload.quantity;
      }
    },
  },
});

export const loadCartAsync = (): AppThunk => async (dispatch) => {
  var { data } = await CartService.loadCartAsync();
  dispatch(loadCart(data));
};

export const addToCartAsync = (product: Product): AppThunk => async (dispatch) => {
  var { data } = await CartService.AddToCartAsync(product.id, 1);
  dispatch(addToCart({ cartItemId: data, product }));
};

export const removeFromCartAsync = (cartItemId: number): AppThunk => async (dispatch) => {
  await CartService.removeFromCart(cartItemId);
  dispatch(removeFromCart(cartItemId));
};

export const { addToCart, removeFromCart, changeQuantity, loadCart } = cart.actions;

export const selectCartItems = (state: RootState) => state.cart.items;
export const selectTotalItemsInCart = (state: RootState) =>
  state.cart.items.map((s) => s.quantity).reduce((accumulator, curr) => accumulator + curr, 0);

export default cart.reducer;

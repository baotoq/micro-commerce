import { createSlice, PayloadAction } from "@reduxjs/toolkit";
import { AppThunk, RootState } from "..";

import { Product, Cart } from "../../models";
import CartService from "../../services/cart-service";
import OderService from "../../services/order-service";

interface CartState extends Cart {}

const initialState: CartState = {
  id: 0,
  items: [],
};

export const cart = createSlice({
  name: "cart",
  initialState,
  reducers: {
    loadCart: (state, { payload }: PayloadAction<Cart>) => {
      if (payload) {
        state.id = payload.id;
        state.items = payload.items;
      } else {
        state.id = 0;
        state.items = [];
      }
    },
    addToCart: (state, { payload }: PayloadAction<{ cartItemId: number; product: Product }>) => {
      const item = state.items.find((s) => s.id === payload.cartItemId);
      if (item) {
        item.quantity += 1;
      } else {
        state.items.push({ id: payload.cartItemId, product: payload.product, quantity: 1 });
      }
    },
    removeFromCart: (state, { payload }: PayloadAction<number>) => {
      state.items = state.items.filter((s) => s.id !== payload);
    },
    changeQuantity: (state, { payload }: PayloadAction<{ cartItemId: number; quantity: number }>) => {
      const item = state.items.find((s) => s.id === payload.cartItemId);
      if (item) {
        item.quantity = payload.quantity;
      }
    },
  },
});

export const loadCartAsync = (): AppThunk => async (dispatch) => {
  const { data } = await CartService.loadCartAsync();
  dispatch(loadCart(data));
};

export const addToCartAsync = (product: Product): AppThunk => async (dispatch) => {
  const { data } = await CartService.AddToCartAsync(product.id, 1);
  dispatch(addToCart({ cartItemId: data, product }));
};

export const removeFromCartAsync = (cartItemId: number): AppThunk => async (dispatch) => {
  await CartService.removeFromCart(cartItemId);
  dispatch(removeFromCart(cartItemId));
};

export const changeQuantityAsync = (cartItemId: number, quantity: number): AppThunk => async (dispatch) => {
  await CartService.updateQuantityAsync(cartItemId, quantity);
  dispatch(changeQuantity({ cartItemId, quantity }));
};

export const createOrderAsync = (): AppThunk => async (dispatch, getState) => {
  await OderService.createAsync(getState().cart.id);
  dispatch(loadCartAsync());
};

export const { addToCart, removeFromCart, changeQuantity, loadCart } = cart.actions;

export const selectCartItems = (state: RootState) => state.cart.items;
export const selectTotalItemsInCart = (state: RootState) =>
  state.cart.items.map((s) => s.quantity).reduce((accumulator, curr) => accumulator + curr, 0);
export const selectSubTotalPricesInCart = (state: RootState) =>
  state.cart.items.map((s) => s.quantity * s.product.price).reduce((accumulator, curr) => accumulator + curr, 0);

export default cart.reducer;

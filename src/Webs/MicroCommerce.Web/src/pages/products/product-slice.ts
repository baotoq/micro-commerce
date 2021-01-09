import { createSlice, PayloadAction } from "@reduxjs/toolkit";
import { RootState } from "~/store";

interface ProductState {
  value: number;
}

const initialState = { value: 0 } as ProductState;

const productSlice = createSlice({
  name: "product",
  initialState,
  reducers: {
    increment(state) {
      state.value++;
    },
    decrement(state) {
      state.value--;
    },
    incrementByAmount(state, action: PayloadAction<number>) {
      state.value += action.payload;
    },
  },
});

export const productReducer = productSlice.reducer;

export const { increment, decrement, incrementByAmount } = productSlice.actions;

export const selectValue = (state: RootState) => state.products.value;

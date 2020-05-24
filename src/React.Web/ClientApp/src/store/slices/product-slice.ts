import { createSlice, PayloadAction } from "@reduxjs/toolkit";
import { AppThunk, RootState } from "../";

export interface Product {
  name: string;
}

interface ProductState {
  product: Product;
}

const initialState: ProductState = {
  product: { name: "" },
};

export const productSlice = createSlice({
  name: "product",
  initialState,
  reducers: {
    fetchProduct: (state, { payload }: PayloadAction<Product>) => {
      state.product = payload;
    },
  },
});

export const { fetchProduct } = productSlice.actions;

export const fetchProductAsync = (id: number): AppThunk => async (dispatch) => {
  dispatch(fetchProduct({ name: "as" } as Product));
};

export default productSlice.reducer;

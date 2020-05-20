import { createSlice, PayloadAction } from "@reduxjs/toolkit";
import { AppThunk, RootState } from "../";

import categoryService from "../../services/category-service";

interface Category {
  id: number;
  name: string;
}

interface CategoriesState {
  categories: Category[];
}

const initialState: CategoriesState = {
  categories: [],
};

export const categories = createSlice({
  name: "categories",
  initialState,
  reducers: {
    fetchCategories: (state, { payload }: PayloadAction<Category[]>) => {
      state.categories = payload;
    },
  },
});

export const { fetchCategories } = categories.actions;

export const fetchCategoriesAsync = (): AppThunk => async (dispatch) => {
  const data = await categoryService.findAll();
  dispatch(fetchCategories(data));
};

export const selectCategories = (state: RootState) => state.categories.categories;

export default categories.reducer;

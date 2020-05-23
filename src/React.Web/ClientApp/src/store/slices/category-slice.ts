import { createSlice, PayloadAction } from "@reduxjs/toolkit";
import { AppThunk, RootState } from "..";

import categoryService from "../../services/category-service";

interface Category {
  id: number;
  name: string;
}

interface CategoryState {
  categories: Category[];
}

const initialState: CategoryState = {
  categories: [],
};

export const category = createSlice({
  name: "category",
  initialState,
  reducers: {
    fetchCategories: (state, { payload }: PayloadAction<Category[]>) => {
      state.categories = payload;
    },
  },
});

export const { fetchCategories } = category.actions;

export const fetchCategoriesAsync = (): AppThunk => async (dispatch) => {
  const data = await categoryService.findAllAsync();
  dispatch(fetchCategories(data));
};

export const selectCategories = (state: RootState) => state.category.categories;

export default category.reducer;

import { createSlice, PayloadAction } from "@reduxjs/toolkit";
import { AppThunk, RootState } from "..";

import categoryService from "../../services/category-service";

interface Category {
  id: number;
  name: string;
}

interface CategoryState {
  categories: Category[];
  activeTab: number;
}

const initialState: CategoryState = {
  categories: [],
  activeTab: 0,
};

export const category = createSlice({
  name: "category",
  initialState,
  reducers: {
    fetchCategories: (state, { payload }: PayloadAction<Category[]>) => {
      state.categories = payload;
    },
    changeActiveTab: (state, { payload }: PayloadAction<number>) => {
      state.activeTab = payload;
    },
  },
});

export const { fetchCategories, changeActiveTab } = category.actions;

export const fetchCategoriesAsync = (): AppThunk => async (dispatch) => {
  const data = await categoryService.findAllAsync();
  dispatch(fetchCategories(data.data));
};

export const selectCategories = (state: RootState) => state.category.categories;
export const selectActiveTab = (state: RootState) => state.category.activeTab;

export default category.reducer;

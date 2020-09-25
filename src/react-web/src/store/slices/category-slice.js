import { createSlice } from "@reduxjs/toolkit";

import CategoryService from "../../services/category-service";

const initialState = {
  categories: [],
};

export const category = createSlice({
  name: "category",
  initialState,
  reducers: {
    fetchCategories: (state, { payload }) => {
      state.categories = payload;
    },
  },
});

export const { fetchCategories, changeActiveTab } = category.actions;

export const fetchCategoriesAsync = () => async (dispatch) => {
  const data = await CategoryService.findAsync();
  dispatch(fetchCategories(data.data));
};

export const selectCategories = (state) => state.category.categories;

export default category.reducer;

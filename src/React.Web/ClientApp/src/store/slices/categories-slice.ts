import { createSlice, PayloadAction } from "@reduxjs/toolkit";
import { AppThunk, RootState } from "../";

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
  dispatch(
    fetchCategories([
      {
        id: 1,
        name: "Apple",
      },
      {
        id: 2,
        name: "Samsung",
      },
      {
        id: 3,
        name: "Nokia",
      },
      {
        id: 4,
        name: "Xiaomi",
      },
      {
        id: 5,
        name: "Huawei",
      },
    ])
  );
};

export const selectCategories = (state: RootState) => state.categories.categories;

export default categories.reducer;

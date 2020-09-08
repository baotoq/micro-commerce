import { createSlice } from "@reduxjs/toolkit";
import { delay, map } from "rxjs/operators";
import { of } from "rxjs";

export const counterSlice = createSlice({
  name: "counter",
  initialState: {
    value: 0,
  },
  reducers: {
    increment: (state) => {
      state.value += 1;
    },
    decrement: (state) => {
      state.value -= 1;
    },
    incrementByAmount: (state, action) => {
      state.value += action.payload;
    },
    incrementAsync: () => {},
  },
});

export const {
  increment,
  decrement,
  incrementByAmount,
  incrementAsync,
} = counterSlice.actions;

// export const incrementAsync = (amount) => (dispatch) => {
//   setTimeout(() => {
//     dispatch(incrementByAmount(amount));
//   }, 1000);
// };

export const selectCount = (state) => state.counter.value;

export default counterSlice.reducer;

export const epics = {
  incrementAsync: (action$, state$, action) =>
    of([]).pipe(
      delay(1000),
      map(() => incrementByAmount(2))
    ),
};

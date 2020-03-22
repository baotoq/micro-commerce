import { VisibilityFilters } from "../actions";
const visibilityFilter = (
  state = VisibilityFilters.SHOW_ALL,
  { type, payload }
) => {
  switch (type) {
    case "SET_VISIBILITY_FILTER":
      return payload.filter;
    default:
      return state;
  }
};
export default visibilityFilter;

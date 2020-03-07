import React from "react";
import PropTypes from "prop-types";
import { useSelector, useDispatch } from "react-redux";
import { setVisibilityFilter } from "../store/actions";
import Link from "../components/Link";

const FilterLink = ({ filter, children }) => {
  const dispatch = useDispatch();
  const visibilityFilter = useSelector(state => state.visibilityFilter);
  const active = filter === visibilityFilter;
  return (
    <Link
      children={children}
      active={active}
      onClick={() => dispatch(setVisibilityFilter(filter))}
    />
  );
};

FilterLink.propTypes = {
  filter: PropTypes.string.isRequired,
  children: PropTypes.string.isRequired
};

export default FilterLink;

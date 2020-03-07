import React, { useEffect } from "react";
import { useSelector, useDispatch } from "react-redux";
import { createSelector } from "reselect";
import { fetchTodos, toggleTodo, VisibilityFilters } from "../store/actions";
import TodoList from "../components/TodoList";

const getVisibleTodos = createSelector(
  state => state.todos,
  state => state.visibilityFilter,
  (todos, filter) => {
    switch (filter) {
      case VisibilityFilters.SHOW_ALL:
        return todos;
      case VisibilityFilters.SHOW_COMPLETED:
        return todos.filter(t => t.completed);
      case VisibilityFilters.SHOW_ACTIVE:
        return todos.filter(t => !t.completed);
      default:
        throw new Error("Unknown filter: " + filter);
    }
  }
);

const VisibleTodoList = () => {
  const dispatch = useDispatch();
  const visibleTodos = useSelector(getVisibleTodos);
  useEffect(() => {
    dispatch(fetchTodos());
  });
  return (
    <div>
      <TodoList
        todos={visibleTodos}
        toggleTodo={id => dispatch(toggleTodo(id))}
      />
    </div>
  );
};

VisibleTodoList.propTypes = {};

export default VisibleTodoList;

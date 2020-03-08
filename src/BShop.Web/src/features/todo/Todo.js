import React from "react";
import AddTodo from "./containers/AddTodo";
import VisibleTodoList from "./containers/VisibleTodoList";
import Filter from "./containers/Filter";

const Todo = () => (
  <div>
    <AddTodo />
    <VisibleTodoList />
    <Filter />
  </div>
);
export default Todo;

import axios from "axios";

let nextTodoId = 0;

export const addTodo = text => ({
  type: "ADD_TODO",
  payload: { id: nextTodoId++, text }
});

export const setVisibilityFilter = filter => ({
  type: "SET_VISIBILITY_FILTER",
  payload: { filter }
});

export const toggleTodo = id => ({
  type: "TOGGLE_TODO",
  payload: { id }
});

export const fetchTodos = () => async dispatch => {
  const {
    data: { todo }
  } = await axios.get("http://localhost:6060/api/t/todos");

  dispatch(
    recieveTodos(todo.map(t => ({ id: t.id, text: t.title, completed: false })))
  );
};

export const VisibilityFilters = {
  SHOW_ALL: "SHOW_ALL",
  SHOW_COMPLETED: "SHOW_COMPLETED",
  SHOW_ACTIVE: "SHOW_ACTIVE"
};

const recieveTodos = todos => ({
  type: "RECEIVE_TODOS",
  payload: { todos }
});

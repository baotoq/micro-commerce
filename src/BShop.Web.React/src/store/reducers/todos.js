const todos = (state = [], { type, payload }) => {
  switch (type) {
    case "RECEIVE_TODOS":
      return [...payload.todos];
    case "ADD_TODO":
      return [
        ...state,
        {
          id: payload.id,
          text: payload.text,
          completed: false
        }
      ];
    case "TOGGLE_TODO":
      return state.map(todo =>
        todo.id === payload.id ? { ...todo, completed: !todo.completed } : todo
      );
    default:
      return state;
  }
};
export default todos;

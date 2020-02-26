import React, { Component } from "react";
import { connect } from "react-redux";
import { fetchTodos, toggleTodo, VisibilityFilters } from "../store/actions";
import TodoList from "../components/TodoList";

export class VisibleTodoList extends Component {
  componentDidMount() {
    this.props.fetchTodos();
  }

  render() {
    const { todos, toggleTodo } = this.props;

    return (
      <div>
        <TodoList todos={todos} toggleTodo={toggleTodo} />
      </div>
    );
  }
}

const getVisibleTodos = (todos, filter) => {
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
};

const mapStateToProps = state => ({
  todos: getVisibleTodos(state.todos, state.visibilityFilter)
});
const mapDispatchToProps = dispatch => ({
  toggleTodo: id => dispatch(toggleTodo(id)),
  fetchTodos: () => dispatch(fetchTodos())
});

export default connect(mapStateToProps, mapDispatchToProps)(VisibleTodoList);

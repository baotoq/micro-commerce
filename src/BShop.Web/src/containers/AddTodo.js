import React, { Component } from "react";
import { connect } from "react-redux";
import { addTodo } from "../store/actions";

export class AddTodo extends Component {
  constructor(props) {
    super(props);
    this.state = {
      input: ""
    };
  }

  render() {
    let { input } = this.state;
    const { dispatch } = this.props;

    return (
      <div>
        <form
          onSubmit={e => {
            e.preventDefault();
            if (!input.value.trim()) {
              return;
            }
            dispatch(addTodo(input.value));
            input.value = "";
          }}
        >
          <input ref={node => (input = node)} />
          <button type="submit">Add Todo</button>
        </form>
      </div>
    );
  }
}

export default connect()(AddTodo);

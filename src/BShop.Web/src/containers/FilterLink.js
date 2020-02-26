import React, { Component } from "react";
import { connect } from "react-redux";
import { setVisibilityFilter } from "../store/actions";
import Link from "../components/Link";

export class FilterLink extends Component {
  render() {
    return <Link {...this.props}/>;
  }
}

const mapStateToProps = (state, ownProps) => ({
  active: ownProps.filter === state.visibilityFilter
});
const mapDispatchToProps = (dispatch, ownProps) => ({
  onClick: () => dispatch(setVisibilityFilter(ownProps.filter))
});
export default connect(mapStateToProps, mapDispatchToProps)(FilterLink);

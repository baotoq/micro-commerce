import React, { useEffect } from "react";
import PropTypes from "prop-types";
import API from "../../shared/api";

const Catalog = props => {
  useEffect(() => {
    async function fetchCatalog() {
      const response = await API.get("");
    }
    fetchCatalog();
  }, [])
  return <div>Catalog</div>;
};

Catalog.propTypes = {};

export default Catalog;

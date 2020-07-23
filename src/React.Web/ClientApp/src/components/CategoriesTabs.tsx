import React from "react";
import { useSelector } from "react-redux";
import { Link } from "react-router-dom";

import { selectCategories, selectActiveTab } from "../store/slices/category-slice";

import AppBar from "@material-ui/core/AppBar";
import Tabs from "@material-ui/core/Tabs";
import Tab from "@material-ui/core/Tab";

const CategoriesTabs = () => {
  const categories = useSelector(selectCategories);
  const activeTab = useSelector(selectActiveTab);

  return (
    <div>
      <AppBar position="static" color="default">
        <Tabs value={activeTab} centered>
          <Tab to="/" component={Link} label="Home" value={0} />
          {categories.map((c) => (
            <Tab to={`/category/${c.id}/page/1`} component={Link} label={c.name} value={c.id} key={c.id} />
          ))}
        </Tabs>
      </AppBar>
    </div>
  );
};

export default CategoriesTabs;

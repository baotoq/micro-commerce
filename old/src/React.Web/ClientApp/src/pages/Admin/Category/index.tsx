import React, { useEffect } from "react";
import { useSelector } from "react-redux";
import MaterialTable, { Column } from "material-table";

import CategoryService from "../../../services/category-service";
import { Category } from "../../../models";

const Index = () => {
  const [columns, setColumns] = React.useState<Column<Category>[]>([{ title: "Name", field: "name" }]);

  return (
    <MaterialTable
      title="Categories"
      columns={columns}
      data={async (query) => {
        const paged = await CategoryService.findAsync();
        return {
          data: paged.data,
          page: query.page,
          totalCount: paged.data.length,
        };
      }}
      options={{
        actionsColumnIndex: -1,
        paging: false,
        search: false,
        addRowPosition: "first"
      }}
      editable={{
        onRowAdd: async (newData) => await CategoryService.createAsync(newData.name),
        onRowUpdate: async (newData, oldData) => {
          if (oldData) {
            await CategoryService.updateAsync(oldData.id, newData.name);
          }
        },
        onRowDelete: async (oldData) => await CategoryService.deleteAsync(oldData.id),
      }}
    />
  );
};

export default Index;

import React from "react";
import MaterialTable, { Column } from "material-table";
import Rating from "@material-ui/lab/Rating";
import Image from "material-ui-image";
import ProductService from "../../../services/product-service";
import { Product } from "../../../models";

const Index = () => {
  const [columns, setColumns] = React.useState<Column<Product>[]>([
    { title: "Name", field: "name" },
    {
      title: "Rating",
      field: "ratingAverage",
      editable: "never",
      render: (rowData) => <Rating size="small" value={rowData.ratingAverage} readOnly />,
    },
    { title: "Review Count", field: "reviewsCount", editable: "never" },
    { title: "Price", field: "price", type: "currency" },
  ]);

  return (
    <MaterialTable
      title="Products"
      columns={columns}
      data={async (query) => {
        const paged = await ProductService.findAsync({
          page: query.page + 1,
          pageSize: query.pageSize,
          queryString: query.search,
        });
        return {
          data: paged.data,
          page: query.page,
          totalCount: paged.totalCount,
        };
      }}
      detailPanel={(rowData) => {
        return (
          <div style={{ padding: "15px" }}>
            <b>Description</b>: {rowData.description}
            <Image src={rowData.imageUri} aspectRatio={16 / 9} />
          </div>
        );
      }}
      options={{
        actionsColumnIndex: -1,
        pageSize: 10,
        pageSizeOptions: [10, 20, 30],
        addRowPosition: "first",
      }}
      editable={{
        onRowAdd: async (newData) => await ProductService.createAsync(newData.name),
        onRowUpdate: async (newData, oldData) => {
          if (oldData) {
            await ProductService.updateAsync(oldData.id, newData.name);
          }
        },
        onRowDelete: async (oldData) => await ProductService.deleteAsync(oldData.id),
      }}
    />
  );
};

export default Index;

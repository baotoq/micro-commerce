import React from "react";
import MaterialTable, { Column } from "material-table";
import Rating from "@material-ui/lab/Rating";
import ReviewService from "../../../services/review-service";
import { Review, ReviewStatus } from "../../../models";

const Index = () => {
  const [columns, setColumns] = React.useState<Column<Review>[]>([
    { title: "Title", field: "title", editable: "never" },
    { title: "Product", field: "productName", editable: "never" },
    { title: "Review By", field: "createdByUserName", editable: "never" },
    {
      title: "Rating",
      field: "rating",
      editable: "never",
      render: (rowData) => <Rating size="small" value={rowData.rating} readOnly />,
    },
    {
      title: "Status",
      field: "reviewStatus",
      lookup: {
        [ReviewStatus.Pending]: "Pending",
        [ReviewStatus.Approved]: "Approved",
        [ReviewStatus.NotApproved]: "Not approved",
      },
    },
  ]);

  return (
    <MaterialTable
      title="Products"
      columns={columns}
      data={async (query) => {
        const paged = await ReviewService.findOffsetAsync({
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
            <b>Comment</b>: {rowData.comment}
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
        onRowUpdate: async (newData, oldData) => {
          if (oldData) {
            await ReviewService.changeStatusAsync(oldData.id, newData.reviewStatus);
          }
        },
        onRowDelete: async (oldData) => await ReviewService.deleteAsync(oldData.id),
      }}
    />
  );
};

export default Index;

import React from "react";
import MaterialTable, { Column } from "material-table";
import OrderService from "../../../services/order-service";
import { Order, OrderStatus } from "../../../models";

const Index = () => {
  const [columns, setColumns] = React.useState<Column<Order>[]>([
    { title: "Order Id", field: "id", editable: "never" },
    { title: "Total", field: "subTotal", type: "currency", editable: "never" },
    {
      title: "Status",
      field: "orderStatus",
      lookup: {
        [OrderStatus.New]: "New",
        [OrderStatus.PaymentReceived]: "Payment Received",
        [OrderStatus.Invoiced]: "Invoiced",
        [OrderStatus.Shipping]: "Shipping",
        [OrderStatus.Completed]: "Completed",
        [OrderStatus.Canceled]: "Canceled",
        [OrderStatus.Closed]: "Closed",
      },
    },
  ]);

  return (
    <MaterialTable
      title="Products"
      columns={columns}
      data={async (query) => {
        const paged = await OrderService.findAsync({
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
        return <div style={{ padding: "15px" }}></div>;
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
            await OrderService.changeOrderStatusAsync(oldData.id, newData.orderStatus);
          }
        },
        onRowDelete: async (oldData) => await OrderService.deleteAsync(oldData.id),
      }}
    />
  );
};

export default Index;

import React from "react";
import MaterialTable, { Column } from "material-table";
import OrderService from "../../../services/order-service";
import { Order, OrderStatus } from "../../../models";

import Box from "@material-ui/core/Box";
import Table from "@material-ui/core/Table";
import TableBody from "@material-ui/core/TableBody";
import TableCell from "@material-ui/core/TableCell";
import TableHead from "@material-ui/core/TableHead";
import TableRow from "@material-ui/core/TableRow";
import Typography from "@material-ui/core/Typography";

const Index = () => {
  const [columns, setColumns] = React.useState<Column<Order>[]>([
    { title: "Order Id", field: "id", editable: "never" },
    { title: "Customer", field: "customerName", editable: "never" },
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
    { title: "Total", field: "subTotal", type: "currency", editable: "never" },
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
        return (
          <div style={{ padding: "15px" }}>
            <Box margin={1}>
              <Typography variant="h6" gutterBottom component="div">
                History
              </Typography>
              <Table size="small" aria-label="purchases">
                <TableHead>
                  <TableRow>
                    <TableCell>Product</TableCell>
                    <TableCell align="right">Quantity</TableCell>
                    <TableCell align="right">Price</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {rowData.orderItems.map((item) => (
                    <TableRow key={item.id}>
                      <TableCell component="th" scope="row">
                        {item.productName}
                      </TableCell>
                      <TableCell align="right">{item.quantity}</TableCell>
                      <TableCell align="right">${item.productPrice}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </Box>
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
            await OrderService.changeOrderStatusAsync(oldData.id, newData.orderStatus);
          }
        },
        onRowDelete: async (oldData) => await OrderService.deleteAsync(oldData.id),
      }}
    />
  );
};

export default Index;

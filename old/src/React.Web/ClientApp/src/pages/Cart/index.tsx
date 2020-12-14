import React, { useEffect, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import MaterialTable, { Column, MTableBody } from "material-table";
import { CartItem } from "../../models";
import {
  selectCartItems,
  selectSubTotalPricesInCart,
  removeFromCartAsync,
  changeQuantityAsync,
  createOrderAsync,
} from "../../store/slices/cart-slice";

import TableCell from "@material-ui/core/TableCell";
import TableRow from "@material-ui/core/TableRow";
import Button from "@material-ui/core/Button";

const Index = () => {
  const dispatch = useDispatch();
  const cartItems = useSelector(selectCartItems);
  const subTotal = useSelector(selectSubTotalPricesInCart);
  const [data, setData] = useState<CartItem[]>([]);

  useEffect(() => {
    setData(cartItems.map((item) => ({ ...item })));
  }, [cartItems]);

  const [columns, setColumns] = useState<Column<CartItem>[]>([
    { title: "Product", field: "product.name", editable: "never" },
    {
      title: "Quantity",
      field: "quantity",
      type: "numeric",
      validate: (rowData) => rowData.quantity <= rowData.product.cartMaxQuantity,
    },
    { title: "Price", field: "product.price", type: "currency", editable: "never" },
  ]);

  return (
    <>
      <MaterialTable
        title="Cart"
        columns={columns}
        data={data}
        options={{
          actionsColumnIndex: -1,
          paging: false,
          search: false,
        }}
        editable={{
          onRowUpdate: async (rowData) => await dispatch(changeQuantityAsync(rowData.id, rowData.quantity)),
          onRowDelete: async (rowData) => await dispatch(removeFromCartAsync(rowData.id)),
        }}
        components={{
          Body: (props) => (
            <>
              <MTableBody {...props} />
              <TableRow>
                <TableCell rowSpan={2} />
                <TableCell colSpan={1}>
                  <b>Sub Total</b>
                </TableCell>
                <TableCell align="right">
                  <b>{subTotal.toFixed(2)}</b>
                </TableCell>
              </TableRow>
            </>
          ),
        }}
      />
      <Button
        color="primary"
        variant="contained"
        disabled={cartItems.length === 0}
        style={{ marginTop: "10px" }}
        onClick={() => dispatch(createOrderAsync())}
      >
        Check out
      </Button>
    </>
  );
};

export default Index;

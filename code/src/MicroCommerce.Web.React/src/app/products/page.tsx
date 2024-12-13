"use client";

import {
  DeleteButton,
  EditButton,
  ImageField,
  List,
  ShowButton,
  useTable,
} from "@refinedev/antd";
import type { BaseRecord } from "@refinedev/core";
import { Space, Table } from "antd";

export default function ProductList() {
  const { tableProps } = useTable({
    syncWithLocation: true,
    dataProviderName: "products",
  });

  return (
    <List>
      <Table {...tableProps} rowKey="id">
        <Table.Column dataIndex="id" title={"Id"} />
        <Table.Column dataIndex="name" title={"Name"} />
        <Table.Column dataIndex="price" title={"Price"} />
        <Table.Column dataIndex="remainingStock" title={"Remaining Stock"} />
        <Table.Column
          title="Image"
          dataIndex="image"
          render={(_, record) => (
            <ImageField
              value={`https://localhost:7477/api/products/images/${record.imageUrl}`}
              title={record.imageUrl}
              width={100}
            />
          )}
          width="25%"
        />
        <Table.Column
          title={"Actions"}
          dataIndex="actions"
          render={(_, record: BaseRecord) => (
            <Space>
              <EditButton hideText size="small" recordItemId={record.id} />
              <ShowButton hideText size="small" recordItemId={record.id} />
              <DeleteButton hideText size="small" recordItemId={record.id} />
            </Space>
          )}
        />
      </Table>
    </List>
  );
}

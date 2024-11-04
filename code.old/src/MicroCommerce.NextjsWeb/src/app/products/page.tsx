"use client";

import {
  DeleteButton,
  EditButton,
  ImageField,
  List,
  ShowButton,
  useTable,
} from "@refinedev/antd";
import { BaseRecord } from "@refinedev/core";
import { Space, Table } from "antd";

interface IProduct {
  id: string;
  name: string;
  image: string;
  imageUrl: string;
}

export default function ProductList() {
  const { tableProps } = useTable({
    syncWithLocation: true,
  });

  return (
    <List>
      <Table {...tableProps} rowKey="id">
        <Table.Column dataIndex="name" title={"Name"} />
        <Table.Column<IProduct>
          title="Image"
          dataIndex="image"
          render={(_, record) => (
            <ImageField
              value={record.imageUrl}
              title={record.name}
              width={50}
            />
          )}
          width="50%"
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

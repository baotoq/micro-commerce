"use client";

import { Create, useForm } from "@refinedev/antd";
import { Form, Input, InputNumber } from "antd";

export default function ProductCreate() {
  const { formProps, saveButtonProps } = useForm({
    dataProviderName: "products",
  });

  return (
    <Create saveButtonProps={saveButtonProps}>
      <Form {...formProps} layout="vertical">
        <Form.Item
          label={"Name"}
          name={["name"]}
          rules={[
            {
              required: true,
            },
          ]}
        >
          <Input />
        </Form.Item>
        <Form.Item
          label={"Price"}
          name={["price"]}
          rules={[
            {
              required: true,
            },
          ]}
        >
          <InputNumber />
        </Form.Item>
        <Form.Item
          label={"Remaining Stock"}
          name={["remainingStock"]}
          rules={[
            {
              required: true,
            },
          ]}
        >
          <InputNumber />
        </Form.Item>
      </Form>
    </Create>
  );
}

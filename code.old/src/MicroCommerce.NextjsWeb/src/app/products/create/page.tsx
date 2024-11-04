"use client";

import { Create, useForm } from "@refinedev/antd";
import { Form, Input, InputNumber } from "antd";

export default function ProductCreate() {
  const { formProps, saveButtonProps } = useForm({});

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
          <InputNumber
            prefix="$"
            min="0"
            precision={2}
            style={{ width: "100%" }}
          />
        </Form.Item>
        <Form.Item
          label={"Stock"}
          name={["remainingStock"]}
          rules={[
            {
              required: true,
            },
          ]}
        >
          <InputNumber min={1} precision={0} style={{ width: "100%" }} />
        </Form.Item>
      </Form>
    </Create>
  );
}

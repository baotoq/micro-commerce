"use client";

import { Create, useForm } from "@refinedev/antd";
import { Form, Input } from "antd";

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
      </Form>
    </Create>
  );
}

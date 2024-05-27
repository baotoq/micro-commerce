"use client";

import { Edit, useForm } from "@refinedev/antd";
import { Form, Input, Upload } from "antd";

export default function ProductEdit() {
  const { formProps, saveButtonProps } = useForm({});

  return (
    <Edit saveButtonProps={saveButtonProps}>
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
          label={"Image"}
          name={["imageUrl"]}
          rules={[
            {
              required: true,
            },
          ]}
        >
        </Form.Item>
      </Form>
    </Edit>
  );
}

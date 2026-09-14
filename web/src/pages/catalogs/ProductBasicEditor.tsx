import { App, Button, Drawer, Form, Input, InputNumber, Select } from "antd";
import { useCreateMutation, useLookupQuery, useUpdateMutation } from "../../api/endpoints";
import { dimensions, errorMessage, productStatuses } from "../../api/objectWorkspace";
import type { ProductRow } from "../../api/productList";

export default function ProductBasicEditor({ product, onClose }: { product: ProductRow | null; onClose: () => void }) {
  const [form] = Form.useForm();
  const { message } = App.useApp();
  const types = useLookupQuery("product-types");
  const [create, creating] = useCreateMutation();
  const [update, updating] = useUpdateMutation();
  const saving = creating.isLoading || updating.isLoading;
  return <Drawer open title={product ? `Характеристики: ${product.productCode}` : "Новое изделие"} size={560}
    onClose={() => { if (!saving) onClose(); }} maskClosable={!saving}
    extra={<Button type="primary" loading={saving} onClick={() => form.submit()}>Сохранить</Button>}>
    <Form form={form} layout="vertical" initialValues={product ?? { status: "Created", ...Object.fromEntries(dimensions.map(([name]) => [name, 0])) }}
      onFinish={async (values) => {
        const body = { ...values, weightKg: values.weightKg ?? null, additionalInfo: values.additionalInfo ?? null };
        try {
          if (product) await update({ path: "products", id: String(product.id), body }).unwrap();
          else await create({ path: "products", body }).unwrap();
          message.success("Изделие сохранено"); onClose();
        } catch (error) { message.error(errorMessage(error)); }
      }}>
      <Form.Item name="productCode" label="Код изделия" rules={[{ required: true, whitespace: true }]}><Input /></Form.Item>
      <Form.Item name="mark" label="Марка" rules={[{ required: true, whitespace: true }]}><Input /></Form.Item>
      <Form.Item name="productTypeId" label="Тип изделия" rules={[{ required: true }]}>
        <Select showSearch optionFilterProp="label" loading={types.isFetching}
          options={types.data?.map((t) => ({ value: Number(t.id), label: t.name }))} />
      </Form.Item>
      {types.isError && <Button onClick={() => types.refetch()}>Повторить загрузку типов</Button>}
      {dimensions.map(([name, label]) => <Form.Item key={name} name={name} label={label} rules={[{ required: true }]}>
        <InputNumber min={0} style={{ width: "100%" }} />
      </Form.Item>)}
      <Form.Item name="weightKg" label="Масса, кг"><InputNumber min={0} style={{ width: "100%" }} /></Form.Item>
      <Form.Item name="status" label="Статус" rules={[{ required: true }]}><Select options={productStatuses} /></Form.Item>
      <Form.Item name="additionalInfo" label="Примечание"><Input.TextArea rows={4} /></Form.Item>
    </Form>
  </Drawer>;
}

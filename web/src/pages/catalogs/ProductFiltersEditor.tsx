import { Alert, Button, DatePicker, Input, InputNumber, Select, Space, Typography } from "antd";
import { DeleteOutlined, PlusOutlined } from "@ant-design/icons";
import dayjs from "dayjs";
import { useState } from "react";
import type { ProductField, ProductFilter } from "../../api/productList";
import { fieldLabels, operatorLabels, valueLabels } from "./productTableSchema";

export default function ProductFiltersEditor({ fields, initial, onApply, onCancel, fixedField }: {
  fields: ProductField[]; initial: ProductFilter[]; onApply: (filters: ProductFilter[]) => void;
  onCancel: () => void; fixedField?: string;
}) {
  const [filters, setFilters] = useState<ProductFilter[]>(initial);
  const [error, setError] = useState("");
  const update = (index: number, patch: Partial<ProductFilter>) => {
    setError(""); setFilters((old) => old.map((f, i) => i === index ? { ...f, ...patch } : f));
  };
  const submit = () => {
    if (filters.some((f) => !f.field || (!["IsEmpty", "IsNotEmpty"].includes(f.operator) && !f.value?.trim()))) {
      setError("Заполните значения условий или удалите незавершённые условия."); return;
    }
    onApply(filters);
  };
  return <Space orientation="vertical" size={12} style={{ width: "100%" }}>
    <Typography.Text type="secondary">Все условия выполняются одновременно. Для диапазона добавьте «От» и «До».
      Условия идентификаторов относятся к одному идентификатору.</Typography.Text>
    {filters.map((filter, index) => {
      const meta = fields.find((f) => f.field === filter.field);
      const noValue = ["IsEmpty", "IsNotEmpty"].includes(filter.operator);
      return <div key={index} className="product-filter-condition">
        {!fixedField && <Select aria-label={`Поле условия ${index + 1}`} showSearch optionFilterProp="label" value={filter.field}
          options={fields.map((f) => ({ value: f.field, label: fieldLabels[f.field] ?? f.field }))}
          onChange={(field) => update(index, { field, operator: fields.find((f) => f.field === field)?.operators[0] ?? "Eq", value: undefined })} />}
        <Space.Compact style={{ width: "100%" }}>
          <Select aria-label={`Операция условия ${index + 1}`} value={filter.operator} style={{ minWidth: 155 }}
            options={meta?.operators.map((op) => ({ value: op, label: operatorLabels[op] }))}
            onChange={(operator) => update(index, { operator, value: undefined })} />
          <Button icon={<DeleteOutlined />} aria-label={`Удалить условие ${index + 1}`} onClick={() => setFilters((old) => old.filter((_, i) => i !== index))} />
        </Space.Compact>
        {!noValue && (meta?.type === "enum" || meta?.type === "boolean" ?
          <Select aria-label={`Значение условия ${index + 1}`} value={filter.value} showSearch optionFilterProp="label" allowClear placeholder="Выберите значение"
            options={(meta.type === "boolean" ? ["true", "false"] : meta.values).map((value) => ({ value, label: valueLabels[value] ?? value }))}
            onChange={(value) => update(index, { value })} /> :
          meta?.type === "date" || meta?.type === "datetime" ?
            <DatePicker aria-label={`Дата условия ${index + 1}`} value={filter.value ? dayjs(filter.value) : null}
              showTime={meta.type === "datetime"} format={meta.type === "datetime" ? "DD.MM.YYYY HH:mm:ss" : "DD.MM.YYYY"}
              onChange={(value) => update(index, { value: value ? meta.type === "date" ? value.format("YYYY-MM-DD") : value.toISOString() : undefined })} /> :
          meta?.type === "number" ?
            <InputNumber<string> stringMode aria-label={`Число условия ${index + 1}`} value={filter.value} placeholder="Введите число" style={{ width: "100%" }}
              onChange={(value) => update(index, { value: value ?? undefined })} /> :
            <Input aria-label={`Текст условия ${index + 1}`} value={filter.value ?? ""} maxLength={2000} placeholder="Введите значение"
              onChange={(e) => update(index, { value: e.target.value })} />)}
      </div>;
    })}
    {error && <Alert type="error" title={error} />}
    <Button icon={<PlusOutlined />} disabled={filters.length >= 50 || fields.length === 0} onClick={() => {
      const field = fixedField ?? fields[0].field;
      setFilters((old) => [...old, { field, operator: fields.find((f) => f.field === field)?.type === "text" ? "Contains" : "Eq" }]);
    }}>Добавить условие</Button>
    <Space wrap>
      <Button type="primary" onClick={submit}>Применить</Button>
      <Button onClick={() => onApply([])}>Сбросить</Button>
      <Button onClick={onCancel}>Отмена</Button>
    </Space>
  </Space>;
}

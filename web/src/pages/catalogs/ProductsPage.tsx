import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useStore } from "react-redux";
import { Alert, App, Button, Checkbox, Drawer, Input, Space, Table, Tag, Typography, Upload } from "antd";
import type { TableColumnsType } from "antd";
import { FilterOutlined, PlusOutlined, ReloadOutlined, SettingOutlined } from "@ant-design/icons";
import dayjs from "dayjs";
import type { RootState } from "../../store";
import { exportCatalog, useImportExcelMutation } from "../../api/endpoints";
import { errorMessage } from "../../api/objectWorkspace";
import { useProductFieldsQuery, useProductListQuery } from "../../api/productList";
import type { ProductField, ProductFilter, ProductListRequest, ProductRow } from "../../api/productList";
import ProductFiltersEditor from "./ProductFiltersEditor";
import ProductBasicEditor from "./ProductBasicEditor";
import { defaultColumns, fieldGroups, fieldLabels, operatorLabels, valueLabels } from "./productTableSchema";

const columnsKey = "dispatcher.products.columns.v1";
function initialColumns(): string[] {
  try {
    const saved: unknown = JSON.parse(localStorage.getItem(columnsKey) ?? "null");
    if (Array.isArray(saved) && saved.every((v) => typeof v === "string")) return [...new Set(["productCode", ...saved])];
  } catch { /* Use defaults when browser storage is unavailable. */ }
  return defaultColumns;
}
function formatValue(value: unknown, field: ProductField): string {
  if (value === null || value === undefined || value === "") return "—";
  if (field.type === "date" || field.type === "datetime") return dayjs(String(value)).format(field.type === "date" ? "DD.MM.YYYY" : "DD.MM.YYYY HH:mm");
  if (field.type === "enum" || field.type === "boolean" || field.field === "bindingSource") return valueLabels[String(value)] ?? String(value);
  if (typeof value === "number") return value.toLocaleString("ru-RU", { maximumFractionDigits: 3, useGrouping: !field.field.endsWith("Id") && field.field !== "id" });
  return String(value);
}
function cellValue(row: ProductRow, field: ProductField): string {
  if (field.field.startsWith("identifiers.")) {
    const key = field.field.slice("identifiers.".length) as keyof ProductRow["identifiers"][number];
    return row.identifiers.map((identifier) => formatValue(identifier[key], field)).join("; ") || "—";
  }
  return formatValue(row[field.field], field);
}

export function ProductsPage() {
  const navigate = useNavigate();
  const { message } = App.useApp();
  const store = useStore<RootState>();
  const [query, setQuery] = useState<ProductListRequest>({ page: 1, pageSize: 20, search: "", sortBy: "productCode", descending: false, filters: [] });
  const [search, setSearch] = useState("");
  const [visible, setVisible] = useState(initialColumns);
  const [panel, setPanel] = useState<"filters" | "columns" | null>(null);
  const [editing, setEditing] = useState<{ product: ProductRow | null } | null>(null);
  const [exporting, setExporting] = useState(false);
  const [importErrors, setImportErrors] = useState<string[]>([]);
  const [importExcel, importing] = useImportExcelMutation();
  const metadata = useProductFieldsQuery();
  const fields = metadata.data ?? [];
  const list = useProductListQuery(query);
  const selectColumns = (next: string[]) => {
    const selected = [...new Set(["productCode", ...next])];
    setVisible(selected);
    try { localStorage.setItem(columnsKey, JSON.stringify(selected)); } catch { /* Session preferences still work. */ }
  };
  const applyFilters = (filters: ProductFilter[]) => {
    if (filters.length > 50) { message.warning("Можно задать не более 50 условий"); return false; }
    setQuery((old) => ({ ...old, page: 1, filters })); return true;
  };
  const makeColumn = (field: ProductField): TableColumnsType<ProductRow>[number] => ({
    key: field.field, title: fieldLabels[field.field] ?? field.field, width: field.field === "additionalInfo" ? 300 : 180,
    fixed: field.field === "productCode" ? "left" : undefined,
    sorter: field.sortable, sortOrder: query.sortBy === field.field ? query.descending ? "descend" : "ascend" : null,
    filteredValue: query.filters.filter((f) => f.field === field.field).map((f) => `${f.operator}:${f.value ?? ""}`),
    filterIcon: (filtered) => <FilterOutlined style={{ color: filtered ? "#1677ff" : undefined }} />,
    filterDropdownProps: { destroyOnHidden: true },
    filterDropdown: ({ close }) => <div style={{ padding: 16, width: 360, maxHeight: "70vh", overflowY: "auto" }} onKeyDown={(e) => e.stopPropagation()}>
      <ProductFiltersEditor key={JSON.stringify(query.filters)} fields={fields} fixedField={field.field}
        initial={query.filters.filter((f) => f.field === field.field)} onCancel={close}
        onApply={(next) => { if (applyFilters([...query.filters.filter((f) => f.field !== field.field), ...next])) close(); }} />
    </div>,
    render: (_value, row) => field.field === "productCode"
      ? <Button type="link" style={{ padding: 0, height: "auto", whiteSpace: "normal", textAlign: "left" }} onClick={() => navigate(`/products/${row.id}`)}>{row.productCode}</Button>
      : <span style={{ overflowWrap: "anywhere" }}>{cellValue(row, field)}</span>,
  });
  const codeField = fields.find((f) => f.field === "productCode");
  const columns: TableColumnsType<ProductRow> = codeField ? [makeColumn(codeField)] : [];
  for (const group of fieldGroups) {
    const children = fields.filter((f) => f.field !== "productCode" && visible.includes(f.field) && f.field in group.fields).map(makeColumn);
    if (children.length) columns.push({ title: group.title, key: group.title, children });
  }
  const other = fields.filter((f) => !fieldLabels[f.field] && visible.includes(f.field)).map(makeColumn);
  if (other.length) columns.push({ title: "Другие сведения", children: other });
  columns.push({ title: "", key: "actions", fixed: "right", width: 115,
    render: (_value, row) => <Button size="small" onClick={() => navigate(`/products/${row.id}`)}>Изменить</Button> });

  return <Space className="products-list" orientation="vertical" size={16} style={{ width: "100%" }}>
    <Typography.Title level={2} style={{ margin: 0 }}>Изделия</Typography.Title>
    <Space wrap>
      <Input.Search aria-label="Поиск изделий" placeholder="Поиск по изделиям" maxLength={256} allowClear value={search} style={{ width: 300 }}
        onChange={(e) => { setSearch(e.target.value); if (!e.target.value) setQuery((old) => ({ ...old, search: "", page: 1 })); }}
        onSearch={(value) => setQuery((old) => ({ ...old, search: value.trim(), page: 1 }))} />
      <Button icon={<FilterOutlined />} onClick={() => setPanel("filters")} disabled={!fields.length}>Фильтры ({query.filters.length})</Button>
      <Button icon={<SettingOutlined />} onClick={() => setPanel("columns")} disabled={!fields.length}>Столбцы</Button>
      <Button icon={<ReloadOutlined />} aria-label="Обновить таблицу" loading={list.isFetching} onClick={() => list.refetch()} />
      <Button type="primary" icon={<PlusOutlined />} onClick={() => setEditing({ product: null })}>Добавить изделие</Button>
    </Space>
    <Space wrap>
      <Upload accept=".xlsx" showUploadList={false} disabled={importing.isLoading} beforeUpload={(file) => {
        setImportErrors([]);
        void importExcel({ path: "products", file }).unwrap().then((result) => {
          setImportErrors(result.errors); message.info(`Создано: ${result.created}, обновлено: ${result.updated}`);
        }).catch((error: unknown) => message.error(errorMessage(error)));
        return false;
      }}><Button loading={importing.isLoading}>Импорт Excel</Button></Upload>
      <Button loading={exporting} onClick={async () => {
        setExporting(true);
        try { await exportCatalog("products", store.getState); } catch (error) { message.error(errorMessage(error)); }
        finally { setExporting(false); }
      }}>Excel: весь каталог</Button>
      <Typography.Text type="secondary">Экспорт основных характеристик без учёта фильтров.</Typography.Text>
    </Space>
    {importErrors.length > 0 && <Alert type="warning" title="Ошибки импорта" description={<div style={{ maxHeight: 200, overflow: "auto" }}>{importErrors.map((error, i) => <div key={i}>{error}</div>)}</div>} closable onClose={() => setImportErrors([])} />}
    {(list.isError || metadata.isError) && <Alert type="error" title="Не удалось загрузить изделия или настройки полей"
      description={errorMessage(list.error ?? metadata.error)} action={<Button onClick={() => { void list.refetch(); void metadata.refetch(); }}>Повторить</Button>} />}
    {(query.filters.length > 0 || query.search) && <Space wrap>
      {query.search && <Tag closable onClose={() => { setSearch(""); setQuery((old) => ({ ...old, search: "", page: 1 })); }}>Поиск: {query.search}</Tag>}
      {query.filters.map((filter, index) => <Tag key={`${index}:${filter.field}`} closable onClose={() => applyFilters(query.filters.filter((_, i) => i !== index))}>
        {fieldLabels[filter.field] ?? filter.field}: {operatorLabels[filter.operator]} {filter.value ? valueLabels[filter.value] ?? filter.value : ""}
      </Tag>)}
      <Button size="small" onClick={() => { setSearch(""); setQuery((old) => ({ ...old, page: 1, search: "", filters: [] })); }}>Сбросить всё</Button>
    </Space>}
    <Typography.Text type="secondary">Дата потребности — ближайшая открытая потребность. Рейс — последнее действующее назначение. Нажмите код для просмотра сведений.</Typography.Text>
    <Table<ProductRow> rowKey="id" bordered size="small" columns={columns} dataSource={list.currentData?.items ?? []}
      loading={list.isFetching || metadata.isLoading} scroll={{ x: "max-content", y: "max(260px, calc(100vh - 390px))" }}
      locale={{ emptyText: list.isError ? "Данные не загружены" : "Изделия не найдены" }}
      pagination={{ current: query.page, pageSize: query.pageSize, total: list.currentData?.total ?? 0, showSizeChanger: true,
        pageSizeOptions: [20, 50, 100, 200], showTotal: (total) => `Всего: ${total}` }}
      onChange={(pagination, _filters, sorter, extra) => {
        const sort = Array.isArray(sorter) ? sorter[0] : sorter;
        if (extra.action === "sort") setQuery((old) => ({ ...old, page: 1, sortBy: sort.order ? String(sort.columnKey) : "productCode", descending: sort.order === "descend" }));
        if (extra.action === "paginate") setQuery((old) => ({ ...old, page: old.pageSize !== pagination.pageSize ? 1 : pagination.current ?? 1, pageSize: pagination.pageSize ?? 20 }));
      }} />
    <Drawer open={panel === "filters"} title="Фильтры изделий" size={560} onClose={() => setPanel(null)} destroyOnHidden>
      {panel === "filters" && <ProductFiltersEditor fields={fields} initial={query.filters} onCancel={() => setPanel(null)}
        onApply={(next) => { if (applyFilters(next)) setPanel(null); }} />}
    </Drawer>
    <Drawer open={panel === "columns"} title="Столбцы таблицы" size={440} onClose={() => setPanel(null)}>
      <Space wrap style={{ marginBottom: 20 }}><Button onClick={() => selectColumns(defaultColumns)}>Основные</Button>
        <Button onClick={() => selectColumns(fields.map((f) => f.field))}>Все столбцы</Button></Space>
      {fieldGroups.map((group) => <div key={group.title} style={{ marginBottom: 20 }}>
        <Typography.Title level={5}>{group.title}</Typography.Title>
        <Space orientation="vertical">{fields.filter((f) => f.field in group.fields).map((field) => <Checkbox key={field.field}
          checked={visible.includes(field.field)} disabled={field.field === "productCode"}
          onChange={(e) => selectColumns(e.target.checked ? [...visible, field.field] : visible.filter((f) => f !== field.field))}>{fieldLabels[field.field]}</Checkbox>)}</Space>
      </div>)}
    </Drawer>
    {editing && <ProductBasicEditor product={editing.product} onClose={() => setEditing(null)} />}
  </Space>;
}

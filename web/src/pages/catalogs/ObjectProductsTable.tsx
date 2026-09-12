import {
  Alert,
  App,
  Button,
  Checkbox,
  Form,
  Input,
  InputNumber,
  Popover,
  Select,
  Space,
  Table,
  Tag,
  Tooltip,
  Typography,
} from "antd";
import {
  DownloadOutlined,
  EditOutlined,
  PlusOutlined,
  ReloadOutlined,
  SettingOutlined,
  SaveOutlined,
  CloseOutlined,
} from "@ant-design/icons";
import type { ColumnsType } from "antd/es/table";
import { useEffect, useMemo, useState } from "react";
import { useBeforeUnload } from "react-router-dom";
import { useLookupQuery } from "../../api/endpoints";
import {
  dimensions,
  errorMessage,
  productStatuses,
  useExportObjectProductsMutation,
  useObjectProductsQuery,
  useObjectStructureQuery,
  useSaveObjectProductMutation,
} from "../../api/objectWorkspace";
import type { ObjectProduct, ProductsQuery } from "../../api/objectWorkspace";

const defaultVisible = [
  "productCode",
  "mark",
  "productTypeName",
  "buildingSectionName",
  "floorName",
  "installationNumber",
  "widthMm",
  "heightMm",
  "thicknessMm",
  "weightKg",
  "status",
  "additionalInfo",
];
const statusLabel = (status: string) =>
  productStatuses.find((x) => x.value === status)?.label ?? status;
const numberText = (value: number | null | undefined) =>
  value == null ? "—" : value.toLocaleString("ru-RU", { maximumFractionDigits: 3 });
const dateText = (value: string) => new Date(value).toLocaleString("ru-RU");

export default function ObjectProductsTable({
  objectId,
  readOnly,
  onEditingChange,
}: {
  objectId: string;
  readOnly: boolean;
  onEditingChange: (editing: boolean) => void;
}) {
  const { message, modal } = App.useApp();
  const [form] = Form.useForm<ObjectProduct>();
  const [query, setQuery] = useState<ProductsQuery>({
    objectId,
    page: 1,
    pageSize: 20,
    sortBy: "productCode",
  });
  const [search, setSearch] = useState("");
  const [editing, setEditing] = useState<ObjectProduct>();
  const [failure, setFailure] = useState<string>();
  const [visible, setVisible] = useState<string[]>(defaultVisible);
  const [selected, setSelected] = useState<React.Key[]>([]);
  const [compact, setCompact] = useState(true);
  const { currentData: data, isFetching, error, refetch } = useObjectProductsQuery(query);
  const { data: structure, error: structureError } = useObjectStructureQuery(objectId);
  const { data: types, error: typesError } = useLookupQuery("product-types");
  const [save, { isLoading: saving }] = useSaveObjectProductMutation();
  const [exportRows, { isLoading: exporting }] = useExportObjectProductsMutation();
  const selectedSection = Form.useWatch("buildingSectionId", form);
  useEffect(() => {
    onEditingChange(!!editing);
  }, [editing, onEditingChange]);
  useBeforeUnload((event) => {
    if (editing) {
      event.preventDefault();
      event.returnValue = "";
    }
  });
  const changeQuery = (patch: Partial<ProductsQuery>) => {
    setSelected([]);
    setQuery((q) => ({ ...q, ...patch, page: patch.page ?? 1 }));
  };
  const beginEdit = (row: ObjectProduct) => {
    setEditing(row);
    setFailure(undefined);
    form.setFieldsValue(row);
  };
  const finish = () => {
    setEditing(undefined);
    setFailure(undefined);
    form.resetFields();
  };
  const cancel = () =>
    modal.confirm({
      title: "Отменить редактирование?",
      content: "Несохранённые значения будут потеряны.",
      okText: "Отменить изменения",
      cancelText: "Продолжить редактирование",
      onOk: finish,
    });
  const saveRow = async () => {
    if (!editing) return;
    try {
      const values = await form.validateFields();
      await save({
        objectId,
        productId: editing.id === -1 ? undefined : editing.id,
        body: {
          ...editing,
          ...values,
          status: editing.status,
          version: editing.version,
          buildingSectionId: values.buildingSectionId ?? null,
          floorId: values.floorId ?? null,
          installationNumber: values.installationNumber ?? null,
          weightKg: values.weightKg ?? null,
        },
      }).unwrap();
      finish();
      setSelected([]);
      message.success("Изделие сохранено");
    } catch (e) {
      if (e && typeof e === "object" && "errorFields" in e) return;
      setFailure(errorMessage(e));
    }
  };
  const add = () =>
    beginEdit({
      id: -1,
      productCode: "",
      productTypeId: 0,
      mark: "",
      status: "Created",
      version: 1,
      widthMm: 0,
      heightMm: 0,
      thicknessMm: 0,
      corniceWidthIncreaseMm: 0,
      totalWidthWithCorniceMm: 0,
      thicknessIncreaseMm: 0,
      rightBendMm: 0,
      leftBendMm: 0,
      claddingWidthWithBendsMm: 0,
      weightKg: null,
      buildingSectionId: null,
      floorId: null,
      installationNumber: null,
      bindingSource: "project",
      createdAt: "",
      updatedAt: "",
    });

  const inputCell = (
    row: ObjectProduct,
    field: keyof ObjectProduct,
    display: React.ReactNode,
    input: React.ReactNode,
    required = false,
    max?: number,
  ) =>
    editing?.id === row.id ? (
      <Form.Item
        name={field}
        style={{ margin: 0 }}
        rules={[
          ...(required ? [{ required: true, message: "Обязательное поле" }] : []),
          ...(max ? [{ max, message: `Не более ${max} символов` }] : []),
        ]}
      >
        {input}
      </Form.Item>
    ) : (
      (display ?? "—")
    );
  const options = types?.map((x) => ({ value: Number(x.id), label: x.name })) ?? [];
  if (editing?.productTypeName && !options.some((x) => x.value === editing.productTypeId))
    options.push({ value: editing.productTypeId, label: editing.productTypeName });
  const columns: ColumnsType<ObjectProduct> = [
    {
      key: "productCode",
      dataIndex: "productCode",
      title: "Код изделия",
      width: 180,
      fixed: "left",
      sorter: true,
      render: (v, row) =>
        row.id === -1 ? (
          inputCell(
            row,
            "productCode",
            v,
            <Input maxLength={64} aria-label="Код изделия" />,
            true,
            64,
          )
        ) : (
          <Typography.Text copyable>{v}</Typography.Text>
        ),
    },
    {
      key: "mark",
      dataIndex: "mark",
      title: "Марка",
      width: 180,
      sorter: true,
      render: (v, row) =>
        inputCell(
          row,
          "mark",
          v,
          <Input maxLength={128} aria-label="Марка" />,
          true,
          128,
        ),
    },
    {
      key: "productTypeName",
      dataIndex: "productTypeName",
      title: "Тип изделия",
      width: 220,
      sorter: true,
      render: (v, row) =>
        inputCell(
          row,
          "productTypeId",
          v,
          <Select
            options={options}
            showSearch
            optionFilterProp="label"
            style={{ minWidth: 180 }}
            aria-label="Тип изделия"
          />,
          true,
        ),
    },
    {
      key: "buildingSectionName",
      dataIndex: "buildingSectionName",
      title: "Секция",
      width: 170,
      render: (v, row) =>
        inputCell(
          row,
          "buildingSectionId",
          v,
          <Select
            allowClear
            options={structure?.sections.map((s) => ({ value: s.id, label: s.name }))}
            style={{ minWidth: 140 }}
            onChange={() => form.setFieldValue("floorId", null)}
            aria-label="Секция изделия"
          />,
        ),
    },
    {
      key: "floorName",
      dataIndex: "floorName",
      title: "Этаж",
      width: 150,
      render: (v, row) =>
        inputCell(
          row,
          "floorId",
          v,
          <Select
            allowClear
            disabled={!selectedSection}
            options={structure?.floors
              .filter((f) => f.buildingSectionId === selectedSection)
              .map((f) => ({ value: f.id, label: f.name }))}
            style={{ minWidth: 120 }}
            aria-label="Этаж изделия"
          />,
        ),
    },
    {
      key: "installationNumber",
      dataIndex: "installationNumber",
      title: "Монтажный №",
      width: 170,
      sorter: true,
      render: (v, row) =>
        inputCell(
          row,
          "installationNumber",
          v,
          <Input maxLength={64} aria-label="Монтажный номер" />,
          false,
          64,
        ),
    },
    ...dimensions.map(([key, title]) => ({
      key,
      dataIndex: key,
      title,
      width: 160,
      sorter: ["widthMm", "heightMm", "thicknessMm"].includes(key),
      render: (v: number, row: ObjectProduct) =>
        inputCell(
          row,
          key,
          numberText(v),
          <InputNumber
            min={["widthMm", "heightMm", "thicknessMm"].includes(key) ? 0.001 : 0}
            precision={3}
            style={{ width: 130 }}
            aria-label={title}
          />,
          true,
        ),
    })),
    {
      key: "weightKg",
      dataIndex: "weightKg",
      title: "Масса, кг",
      width: 160,
      sorter: true,
      render: (v, row) =>
        inputCell(
          row,
          "weightKg",
          numberText(v),
          <InputNumber
            min={0}
            precision={3}
            style={{ width: 130 }}
            aria-label="Масса, кг"
          />,
        ),
    },
    {
      key: "status",
      dataIndex: "status",
      title: "Статус",
      width: 180,
      sorter: true,
      render: (v) => (
        <Tooltip title="Статус меняется операциями производства и логистики">
          <Tag color={v === "Created" ? "blue" : v === "Delivered" ? "green" : "default"}>
            {statusLabel(v)}
          </Tag>
        </Tooltip>
      ),
    },
    {
      key: "additionalInfo",
      dataIndex: "additionalInfo",
      title: "Примечание",
      width: 300,
      render: (v, row) =>
        inputCell(
          row,
          "additionalInfo",
          <Typography.Paragraph
            ellipsis={{ rows: 2, expandable: true }}
            style={{ margin: 0 }}
          >
            {v || "—"}
          </Typography.Paragraph>,
          <Input.TextArea
            autoSize={{ minRows: 1, maxRows: 5 }}
            maxLength={2000}
            aria-label="Примечание"
          />,
          false,
          2000,
        ),
    },
    {
      key: "bindingSource",
      dataIndex: "bindingSource",
      title: "Привязка",
      width: 180,
      render: (v) => (v === "assignment" ? "Текущее назначение" : "По проекту"),
    },
    {
      key: "createdAt",
      dataIndex: "createdAt",
      title: "Создано",
      width: 180,
      render: (v) => (v ? dateText(v) : "—"),
    },
    {
      key: "updatedAt",
      dataIndex: "updatedAt",
      title: "Обновлено",
      width: 180,
      sorter: true,
      render: (v) => (v ? dateText(v) : "—"),
    },
    { key: "version", dataIndex: "version", title: "Версия", width: 90 },
  ];
  const displayedColumns: ColumnsType<ObjectProduct> = [
    ...columns
      .filter((c) => editing || visible.includes(String(c.key)))
      .map((c) => ({
        ...c,
        sortOrder:
          c.sorter && query.sortBy === c.key
            ? query.descending
              ? ("descend" as const)
              : ("ascend" as const)
            : null,
      })),
    {
      key: "actions",
      title: "Действия",
      width: 190,
      fixed: "right",
      render: (_, row) =>
        editing?.id === row.id ? (
          <Space>
            <Button
              type="primary"
              icon={<SaveOutlined />}
              onClick={saveRow}
              loading={saving}
              aria-label="Сохранить изделие"
            />
            <Button
              icon={<CloseOutlined />}
              onClick={cancel}
              disabled={saving}
              aria-label="Отменить редактирование"
            />
          </Space>
        ) : (
          <Button
            icon={<EditOutlined />}
            disabled={readOnly || !!editing || !!structureError || !!typesError}
            onClick={() => beginEdit(row)}
          >
            Изменить
          </Button>
        ),
    },
  ];
  const rows =
    editing?.id === -1 ? [editing, ...(data?.items ?? [])] : (data?.items ?? []);
  const selectedRows = useMemo(
    () => (data?.items ?? []).filter((r) => selected.includes(r.id)),
    [data, selected],
  );
  return (
    <Space
      className="object-products"
      orientation="vertical"
      size={12}
      style={{ width: "100%", minWidth: 0 }}
    >
      <Typography.Text type="secondary">
        Текущие назначения имеют приоритет над проектной привязкой. Размеры — в мм, масса
        — в кг. Код и статус существующего изделия доступны только для чтения.
      </Typography.Text>
      <Space wrap>
        <Input.Search
          aria-label="Поиск изделий"
          placeholder="Код, марка, монтажный №, примечание"
          value={search}
          style={{ width: 350 }}
          allowClear
          disabled={!!editing}
          onChange={(e) => {
            setSearch(e.target.value);
            if (!e.target.value) changeQuery({ search: undefined });
          }}
          onSearch={(value) => changeQuery({ search: value })}
        />
        <Select
          placeholder="Тип изделия"
          aria-label="Фильтр по типу"
          style={{ width: 200 }}
          allowClear
          showSearch
          optionFilterProp="label"
          options={options}
          value={query.productTypeId}
          disabled={!!editing}
          onChange={(value) => changeQuery({ productTypeId: value })}
        />
        <Select
          placeholder="Статус"
          aria-label="Фильтр по статусу"
          style={{ width: 180 }}
          allowClear
          options={productStatuses}
          value={query.status}
          disabled={!!editing}
          onChange={(value) => changeQuery({ status: value })}
        />
        <Select
          placeholder="Секция"
          aria-label="Фильтр по секции"
          style={{ width: 160 }}
          allowClear
          options={structure?.sections.map((s) => ({ value: s.id, label: s.name }))}
          value={query.buildingSectionId}
          disabled={!!editing}
          onChange={(value) =>
            changeQuery({ buildingSectionId: value, floorId: undefined })
          }
        />
        <Select
          placeholder="Этаж"
          aria-label="Фильтр по этажу"
          style={{ width: 150 }}
          allowClear
          options={structure?.floors
            .filter(
              (f) =>
                !query.buildingSectionId ||
                f.buildingSectionId === query.buildingSectionId,
            )
            .map((f) => ({
              value: f.id,
              label: `${f.name} (${structure.sections.find((s) => s.id === f.buildingSectionId)?.name})`,
            }))}
          value={query.floorId}
          disabled={!!editing}
          onChange={(value) => changeQuery({ floorId: value })}
        />
        <Button
          disabled={!!editing}
          onClick={() => {
            setSearch("");
            setSelected([]);
            setQuery({
              objectId,
              page: 1,
              pageSize: query.pageSize,
              sortBy: "productCode",
            });
          }}
        >
          Сбросить фильтры
        </Button>
      </Space>
      <Space wrap style={{ justifyContent: "space-between", width: "100%" }}>
        <Space wrap>
          <Button
            type="primary"
            icon={<PlusOutlined />}
            disabled={readOnly || !!editing || !structure || !!typesError}
            onClick={add}
          >
            Добавить изделие
          </Button>
          <Button
            icon={<ReloadOutlined />}
            disabled={!!editing}
            loading={isFetching}
            onClick={() => void refetch()}
          >
            Обновить
          </Button>
          <Button
            icon={<DownloadOutlined />}
            disabled={!!editing || !!error}
            loading={exporting}
            onClick={() =>
              void exportRows(query)
                .unwrap()
                .catch((e) => message.error(errorMessage(e)))
            }
          >
            Excel по фильтрам
          </Button>
          <Popover
            trigger="click"
            title="Столбцы таблицы"
            content={
              <Checkbox.Group
                value={visible}
                style={{ display: "grid", gap: 6 }}
                options={columns.map((c) => ({
                  value: String(c.key),
                  label: String(c.title),
                  disabled: c.key === "productCode",
                }))}
                onChange={(values) => setVisible(values as string[])}
              />
            }
          >
            <Button icon={<SettingOutlined />} disabled={!!editing}>
              Столбцы
            </Button>
          </Popover>
          <Checkbox checked={compact} onChange={(e) => setCompact(e.target.checked)}>
            Компактно
          </Checkbox>
        </Space>
        <Typography.Text>
          Найдено: <strong>{data?.total ?? "—"}</strong>
        </Typography.Text>
      </Space>
      {selected.length > 0 && (
        <Alert
          type="info"
          title={`Выбрано: ${selectedRows.length} · Масса выбранных: ${numberText(selectedRows.reduce((s, r) => s + (r.weightKg ?? 0), 0))} кг`}
          action={
            <Button size="small" onClick={() => setSelected([])}>
              Снять выбор
            </Button>
          }
        />
      )}
      {editing && (
        <Alert
          type="info"
          title="Редактирование строки"
          description="Сохраните или отмените изменения перед сменой страницы и фильтров. Все редактируемые столбцы раскрыты; таблица прокручивается горизонтально."
        />
      )}
      {failure && <Alert type="error" showIcon title={failure} />}
      {(error || structureError || typesError) && (
        <Alert
          type="error"
          showIcon
          title="Не удалось загрузить данные таблицы или справочники. Обновите страницу."
        />
      )}
      <Form form={form} component={false} disabled={saving}>
        <Table<ObjectProduct>
          rowKey="id"
          size={compact ? "small" : "middle"}
          bordered
          sticky
          columns={displayedColumns}
          dataSource={rows}
          loading={isFetching}
          scroll={{ x: "max-content", y: 560 }}
          rowClassName={(row) => (row.id === editing?.id ? "object-product-editing" : "")}
          rowSelection={{
            selectedRowKeys: selected,
            onChange: setSelected,
            getCheckboxProps: () => ({ disabled: !!editing }),
          }}
          pagination={{
            current: query.page,
            pageSize: query.pageSize,
            total: data?.total ?? 0,
            showSizeChanger: true,
            pageSizeOptions: [20, 50, 100, 200],
            disabled: !!editing,
            showTotal: (total, range) => `${range[0]}–${range[1]} из ${total}`,
          }}
          onChange={(pagination, _filters, sorter) => {
            if (editing) return;
            const s = Array.isArray(sorter) ? sorter[0] : sorter;
            changeQuery({
              page: pagination.current ?? 1,
              pageSize: pagination.pageSize ?? 20,
              sortBy: s.order ? String(s.field) : "productCode",
              descending: s.order === "descend",
            });
          }}
          locale={{
            emptyText: error ? "Ошибка загрузки" : "Изделий по выбранным условиям нет",
          }}
        />
      </Form>
    </Space>
  );
}

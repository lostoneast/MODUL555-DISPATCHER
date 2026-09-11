import { useMemo, useRef, useState } from "react";
import {
  App,
  Button,
  DatePicker,
  Drawer,
  Form,
  Input,
  InputNumber,
  Popconfirm,
  Select,
  Space,
  Switch,
  Table,
  Typography,
  Upload,
} from "antd";
import {
  DownloadOutlined,
  PlusOutlined,
  UploadOutlined,
} from "@ant-design/icons";
import type { ColumnsType } from "antd/es/table";
import dayjs from "dayjs";
import { useStore } from "react-redux";
import {
  exportCatalog,
  useActivateMutation,
  useCreateMutation,
  useImportExcelMutation,
  useListQuery,
  useLookupQuery,
  useRemoveMutation,
  useUpdateMutation,
} from "../api/endpoints";
import type { RootState } from "../store";

export interface FieldDef {
  name: string;
  label: string;
  type?:
    | "text"
    | "number"
    | "textarea"
    | "switch"
    | "select"
    | "lookup"
    | "date";
  options?: { label: string; value: string | number | boolean }[];
  lookup?: string;
  required?: boolean;
}

interface Props {
  title: string;
  path: string;
  columns: ColumnsType<Record<string, unknown>>;
  fields: FieldDef[];
  creatable?: boolean;
  deletable?: boolean;
  showInactive?: boolean;
  drawerWidth?: number;
}

function LookupSelect({
  lookup,
  ...rest
}: { lookup: string } & Record<string, unknown>) {
  const { data, isFetching, isLoading } = useLookupQuery(lookup, {
    refetchOnMountOrArgChange: true,
  });
  const options = useMemo(
    () =>
      (data ?? []).map((x) => ({
        value: /^\d+$/.test(x.id) ? Number(x.id) : x.id,
        label: x.code ? `${x.code} · ${x.name}` : x.name,
      })),
    [data],
  );
  return (
    <Select
      showSearch
      optionFilterProp="label"
      loading={isLoading || isFetching}
      options={options}
      allowClear
      {...rest}
    />
  );
}

export default function CatalogPage({
  title,
  path,
  columns,
  fields,
  creatable = true,
  deletable = true,
  showInactive = true,
  drawerWidth = 480,
}: Props) {
  const { message } = App.useApp();
  const store = useStore<RootState>();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [open, setOpen] = useState(false);
  const [current, setCurrent] = useState<Record<string, unknown> | null>(null);
  const fileRef = useRef<HTMLInputElement>(null);
  const extra = showInactive ? "activeOnly=false" : undefined;
  const { data, isFetching } = useListQuery(
    { path, page, search, extra },
    { refetchOnMountOrArgChange: true },
  );
  const [create, createState] = useCreateMutation();
  const [update, updateState] = useUpdateMutation();
  const [remove] = useRemoveMutation();
  const [activate] = useActivateMutation();
  const [importExcel, importState] = useImportExcelMutation();
  const [form] = Form.useForm();

  const onEdit = (row: Record<string, unknown>) => {
    setCurrent(row);
    const values: Record<string, unknown> = {
      ...row,
      isActive: row.isActive !== false,
    };
    for (const f of fields) {
      if (f.type === "date" && values[f.name])
        values[f.name] = dayjs(String(values[f.name]));
    }
    form.setFieldsValue(values);
    setOpen(true);
  };

  const save = async () => {
    const raw = await form.validateFields();
    const values: Record<string, unknown> = { ...raw };
    for (const f of fields) {
      if (f.type === "date" && values[f.name] && dayjs.isDayjs(values[f.name])) {
        values[f.name] = (values[f.name] as dayjs.Dayjs).format("YYYY-MM-DD");
      }
    }
    try {
      if (current)
        await update({
          path,
          id: String(current.id),
          body: { ...current, ...values },
        }).unwrap();
      else await create({ path, body: values }).unwrap();
      setOpen(false);
      message.success("Сохранено");
    } catch {
      message.error("Ошибка сохранения");
    }
  };

  const onExport = async () => {
    try {
      await exportCatalog(path, store.getState);
      message.success("Экспорт выполнен");
    } catch {
      message.error("Ошибка экспорта");
    }
  };

  const onImportFile = async (file: File) => {
    try {
      const result = await importExcel({ path, file }).unwrap();
      const errCount = result.errors?.length ?? 0;
      message.success(
        `Импорт: создано ${result.created}, обновлено ${result.updated}` +
          (errCount ? `, ошибок ${errCount}` : ""),
      );
      if (errCount)
        message.warning(result.errors.slice(0, 5).join("; "), 8);
    } catch {
      message.error("Ошибка импорта");
    }
    return false;
  };

  return (
    <>
      <Space
        style={{
          width: "100%",
          justifyContent: "space-between",
          marginBottom: 16,
        }}
        wrap
      >
        <Typography.Title level={3} style={{ margin: 0 }}>
          {title}
        </Typography.Title>
        <Space wrap>
          <Input.Search
            allowClear
            placeholder="Поиск"
            onSearch={(v) => {
              setSearch(v);
              setPage(1);
            }}
            style={{ width: 220 }}
          />
          <Button icon={<DownloadOutlined />} onClick={onExport}>
            Экспорт
          </Button>
          <Upload
            accept=".xlsx,.xls"
            showUploadList={false}
            beforeUpload={(file) => {
              void onImportFile(file);
              return false;
            }}
          >
            <Button icon={<UploadOutlined />} loading={importState.isLoading}>
              Импорт
            </Button>
          </Upload>
          <input ref={fileRef} type="file" hidden accept=".xlsx,.xls" />
          {creatable && (
            <Button
              type="primary"
              icon={<PlusOutlined />}
              onClick={() => {
                setCurrent(null);
                form.resetFields();
                form.setFieldsValue({ isActive: true });
                setOpen(true);
              }}
            >
              Добавить
            </Button>
          )}
        </Space>
      </Space>
      <Table
        rowKey="id"
        loading={isFetching}
        dataSource={(data?.items ?? []) as Record<string, unknown>[]}
        columns={[
          ...columns,
          ...(creatable
            ? [
                {
                  title: "",
                  width: deletable ? 200 : 110,
                  render: (_: unknown, row: Record<string, unknown>) => (
                    <Space>
                      <Button size="small" onClick={() => onEdit(row)}>
                        Изменить
                      </Button>
                      {deletable &&
                        (row.isActive !== false ? (
                          <Popconfirm
                            title="Снять с использования?"
                            onConfirm={() =>
                              remove({ path, id: String(row.id) })
                            }
                          >
                            <Button size="small" danger>
                              Отключить
                            </Button>
                          </Popconfirm>
                        ) : (
                          <Button
                            size="small"
                            onClick={() =>
                              activate({ path, id: String(row.id) })
                            }
                          >
                            Включить
                          </Button>
                        ))}
                    </Space>
                  ),
                } as ColumnsType<Record<string, unknown>>[number],
              ]
            : []),
        ]}
        pagination={{
          current: page,
          pageSize: data?.pageSize ?? 20,
          total: data?.total ?? 0,
          onChange: setPage,
          showSizeChanger: false,
        }}
      />
      <Drawer
        title={current ? "Изменить запись" : "Новая запись"}
        open={open}
        onClose={() => setOpen(false)}
        size={drawerWidth}
        extra={
          <Button
            type="primary"
            loading={createState.isLoading || updateState.isLoading}
            onClick={save}
          >
            Сохранить
          </Button>
        }
      >
        <Form form={form} layout="vertical">
          {fields.map((f) => (
            <Form.Item
              key={f.name}
              name={f.name}
              label={f.label}
              valuePropName={f.type === "switch" ? "checked" : "value"}
              rules={
                f.required
                  ? [{ required: true, message: "Обязательное поле" }]
                  : undefined
              }
            >
              {f.type === "textarea" ? (
                <Input.TextArea rows={3} />
              ) : f.type === "number" ? (
                <InputNumber style={{ width: "100%" }} />
              ) : f.type === "switch" ? (
                <Switch />
              ) : f.type === "select" ? (
                <Select options={f.options} allowClear />
              ) : f.type === "lookup" && f.lookup ? (
                <LookupSelect lookup={f.lookup} />
              ) : f.type === "date" ? (
                <DatePicker style={{ width: "100%" }} />
              ) : (
                <Input />
              )}
            </Form.Item>
          ))}
          {creatable &&
            deletable &&
            !fields.some((f) => f.name === "isActive") && (
              <Form.Item
                name="isActive"
                label="Активна"
                valuePropName="checked"
                initialValue={true}
              >
                <Switch />
              </Form.Item>
            )}
        </Form>
      </Drawer>
    </>
  );
}

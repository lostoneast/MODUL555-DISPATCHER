import { Card, Descriptions, Table, Tag, Typography, Alert } from "antd";
import type { ColumnsType } from "antd/es/table";
import { usePrivateDataQuery } from "../api/endpoints";
import type { PrivateDataItem } from "../api/endpoints";
import { brand } from "../theme";

const statusColor: Record<string, string> = {
  "На складе": "blue",
  "В производстве": "orange",
  Доставлено: "green",
};

const columns: ColumnsType<PrivateDataItem> = [
  {
    title: "ID",
    dataIndex: "id",
    width: 80,
  },
  {
    title: "Наименование",
    dataIndex: "name",
  },
  {
    title: "Статус",
    dataIndex: "status",
    render: (status: string) => (
      <Tag color={statusColor[status] ?? "default"}>{status}</Tag>
    ),
  },
];

export default function ProductsPage() {
  const { data, isLoading, isError, error } = usePrivateDataQuery();

  const errMsg =
    error && "data" in error && error.data && typeof error.data === "object"
      ? String((error.data as { message?: string }).message ?? "Ошибка загрузки")
      : isError
        ? "Не удалось загрузить данные"
        : null;

  return (
    <div>
      <Typography.Title level={3} style={{ color: brand.deepBlue, marginTop: 0 }}>
        Изделия
      </Typography.Title>
      <Typography.Paragraph type="secondary">
        Данные из защищённого API (`GET /api/data/private`)
      </Typography.Paragraph>

      {errMsg ? (
        <Alert type="error" showIcon message={errMsg} style={{ marginBottom: 16 }} />
      ) : null}

      <Card style={{ marginBottom: 16 }}>
        <Descriptions size="small" column={{ xs: 1, sm: 2, md: 3 }}>
          <Descriptions.Item label="Пользователь">
            {data?.user ?? "—"}
          </Descriptions.Item>
          <Descriptions.Item label="Авторизован">
            {data?.authenticated ? "да" : "нет"}
          </Descriptions.Item>
          <Descriptions.Item label="Роли">
            {data?.roles?.length
              ? data.roles.map((r) => (
                  <Tag key={r} style={{ marginInlineEnd: 4 }}>
                    {r}
                  </Tag>
                ))
              : "—"}
          </Descriptions.Item>
        </Descriptions>
      </Card>

      <Card>
        <Table<PrivateDataItem>
          rowKey="id"
          loading={isLoading}
          columns={columns}
          dataSource={data?.data ?? []}
          pagination={false}
          size="middle"
        />
      </Card>
    </div>
  );
}

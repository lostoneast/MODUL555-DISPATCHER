import { useState } from "react";
import {
  App,
  Button,
  Card,
  Col,
  Input,
  Popconfirm,
  Row,
  Space,
  Statistic,
  Table,
  Tag,
  Typography,
} from "antd";
import { Link } from "react-router-dom";
import { useSelector } from "react-redux";
import {
  useAdminAuditQuery,
  useAdminOverviewQuery,
  useAdminRolesQuery,
  useClearDemoMutation,
  useSeedDemoMutation,
} from "../api/endpoints";
import type { RootState } from "../store";
import { brand } from "../theme";

export function AdminHomePage() {
  const { message } = App.useApp();
  const user = useSelector((s: RootState) => s.auth.user);
  const { data, isFetching, refetch } = useAdminOverviewQuery();
  const [clearDemo, clearState] = useClearDemoMutation();
  const [seedDemo, seedState] = useSeedDemoMutation();

  const cards = [
    { title: "Изделия", value: data?.products, to: "/" },
    { title: "Типы изделий", value: data?.productTypes, to: "/product-types" },
    { title: "Заводы", value: data?.plants, to: "/plants" },
    { title: "Линии", value: data?.productionLines, to: "/production-lines" },
    {
      title: "Объекты",
      value: data?.constructionObjects,
      to: "/construction-objects",
    },
    { title: "Такты", value: data?.constructionTakts, to: "/construction-takts" },
    { title: "Транспорт", value: data?.vehicles, to: "/vehicles" },
    { title: "Склады", value: data?.storageAreas, to: "/storage-areas" },
    { title: "Рейсы", value: data?.trips },
    { title: "События аудита", value: data?.auditEvents, to: "/admin/audit" },
  ];

  return (
    <Space direction="vertical" size={16} style={{ width: "100%" }}>
      <div>
        <Typography.Title level={3} style={{ margin: 0, color: brand.deepBlue }}>
          Администрирование
        </Typography.Title>
        <Typography.Paragraph type="secondary" style={{ marginBottom: 0 }}>
          Обзор системы. Доступ: роли admin и manager.
          {user ? (
            <>
              {" "}
              Вы вошли как <strong>{user.fullName}</strong> (
              {user.roles?.join(", ") || "без ролей"}).
            </>
          ) : null}
        </Typography.Paragraph>
      </div>

      <Card title="Демонстрационные данные" size="small">
        <Typography.Paragraph type="secondary">
          Сброс очищает все таблицы. Заполнение вставляет набор из кода
          (`DemoDataController.SeedCoreAsync`) — правится на бэке без Excel.
        </Typography.Paragraph>
        <Space wrap>
          <Popconfirm
            title="Очистить все данные?"
            description="Будут удалены справочники, изделия, рейсы и аудит. Действие необратимо."
            okText="Очистить"
            okButtonProps={{ danger: true }}
            onConfirm={async () => {
              try {
                const r = await clearDemo().unwrap();
                message.success(r.message);
                void refetch();
              } catch (e: unknown) {
                const err = e as { data?: { message?: string } };
                message.error(err?.data?.message ?? "Не удалось очистить");
              }
            }}
          >
            <Button danger loading={clearState.isLoading}>
              Сбросить данные
            </Button>
          </Popconfirm>
          <Popconfirm
            title="Заполнить демо-данными?"
            description="База должна быть пустой. Иначе сначала сбросьте данные."
            okText="Заполнить"
            onConfirm={async () => {
              try {
                const r = await seedDemo().unwrap();
                message.success(r.message);
                void refetch();
              } catch (e: unknown) {
                const err = e as { data?: { message?: string } };
                message.error(err?.data?.message ?? "Не удалось заполнить");
              }
            }}
          >
            <Button type="primary" loading={seedState.isLoading}>
              Заполнить демо-данными
            </Button>
          </Popconfirm>
        </Space>
      </Card>

      <Row gutter={[16, 16]}>
        {cards.map((c) => (
          <Col xs={12} sm={8} md={6} key={c.title}>
            <Card size="small" loading={isFetching}>
              <Statistic
                title={
                  c.to ? (
                    <Link to={c.to} style={{ color: "inherit" }}>
                      {c.title}
                    </Link>
                  ) : (
                    c.title
                  )
                }
                value={c.value ?? 0}
              />
            </Card>
          </Col>
        ))}
      </Row>

      <Card size="small" title="Разделы">
        <ul style={{ margin: 0, paddingLeft: 18, lineHeight: 1.8 }}>
          <li>
            <Link to="/admin/roles">Роли Keycloak</Link> — справочник ролей
            системы
          </li>
          <li>
            <Link to="/admin/audit">Журнал аудита</Link> — история изменений
          </li>
          <li>Справочники редактируются в основных разделах меню</li>
        </ul>
      </Card>
    </Space>
  );
}

export function AdminRolesPage() {
  const { data, isFetching } = useAdminRolesQuery();
  return (
    <>
      <Typography.Title level={3} style={{ marginTop: 0, color: brand.deepBlue }}>
        Роли
      </Typography.Title>
      <Typography.Paragraph type="secondary">
        Роли выдаются в Keycloak (realm stroy-company). Учётные записи в БД
        Диспетчера не хранятся.
      </Typography.Paragraph>
      <Table
        rowKey="value"
        loading={isFetching}
        pagination={false}
        dataSource={data ?? []}
        columns={[
          {
            title: "Код",
            dataIndex: "value",
            width: 160,
            render: (v: string) => <Tag>{v}</Tag>,
          },
          { title: "Название", dataIndex: "label" },
          {
            title: "Админ-доступ",
            width: 140,
            render: (_: unknown, row: { value: string }) =>
              row.value === "admin" || row.value === "manager" ? (
                <Tag color="blue">да</Tag>
              ) : (
                <Tag>нет</Tag>
              ),
          },
        ]}
      />
    </>
  );
}

export function AuditAdminPage() {
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const { data, isFetching } = useAdminAuditQuery({ page, search });

  return (
    <>
      <Space
        style={{
          width: "100%",
          justifyContent: "space-between",
          marginBottom: 16,
        }}
      >
        <Typography.Title level={3} style={{ margin: 0, color: brand.deepBlue }}>
          Журнал аудита
        </Typography.Title>
        <Input.Search
          allowClear
          placeholder="Поиск"
          onSearch={(v) => {
            setSearch(v);
            setPage(1);
          }}
          style={{ width: 280 }}
        />
      </Space>
      <Table
        rowKey="id"
        loading={isFetching}
        size="small"
        dataSource={data?.items ?? []}
        columns={[
          {
            title: "Когда",
            dataIndex: "timestamp",
            width: 180,
            render: (v: string) => v?.replace("T", " ").slice(0, 19),
          },
          { title: "Кто", dataIndex: "userName", width: 140 },
          { title: "Сущность", dataIndex: "entityType", width: 160 },
          { title: "Id", dataIndex: "entityId", width: 120, ellipsis: true },
          { title: "Действие", dataIndex: "action", width: 120 },
          {
            title: "Источник",
            dataIndex: "source",
            width: 100,
            render: (v: string) => <Tag>{v}</Tag>,
          },
          { title: "Комментарий", dataIndex: "comment", ellipsis: true },
        ]}
        pagination={{
          current: page,
          total: data?.total,
          pageSize: data?.pageSize ?? 20,
          onChange: setPage,
          showSizeChanger: false,
        }}
      />
    </>
  );
}

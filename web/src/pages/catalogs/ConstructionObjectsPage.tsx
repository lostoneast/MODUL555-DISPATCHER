import {
  App,
  Button,
  Card,
  Checkbox,
  Col,
  DatePicker,
  Drawer,
  Form,
  Input,
  InputNumber,
  Pagination,
  Alert,
  Row,
  Space,
  Tag,
  Typography,
} from "antd";
import {
  PlusOutlined,
  BuildOutlined,
  EnvironmentOutlined,
  CalendarOutlined,
} from "@ant-design/icons";
import dayjs from "dayjs";
import { useEffect, useMemo, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { useCreateMutation, useListQuery } from "../../api/endpoints";

const statusOrder = ["current", "planned", "completed"] as const;

type ConstructionStatus = (typeof statusOrder)[number];

type ConstructionObjectCard = {
  id: number;
  name: string;
  address: string;
  floorsCount: number;
  sectionsCount: number;
  taktsCount: number;
  startDate?: string;
  endDate?: string;
  status?: ConstructionStatus;
};

function buildStatus(startDate?: string, endDate?: string): ConstructionStatus {
  if (!startDate && !endDate) return "current";

  const start = startDate ? dayjs(startDate) : dayjs();
  const end = endDate ? dayjs(endDate) : dayjs();
  const now = dayjs();

  if (now.isAfter(end, "day")) return "completed";
  if (now.isBefore(start, "day")) return "planned";
  return "current";
}

function getStatusMeta(status: ConstructionStatus) {
  const tone = {
    current: { color: "blue", title: "В работе" },
    planned: { color: "gold", title: "Планируется" },
    completed: { color: "green", title: "Завершён" },
  } as const;

  return tone[status];
}

export function ConstructionObjectsPage({ autoOpen = false }: { autoOpen?: boolean } = {}) {
  const { message } = App.useApp();
  const navigate = useNavigate();
  const location = useLocation();
  const [form] = Form.useForm();
  const [open, setOpen] = useState(Boolean(autoOpen));
  const [pendingStatus, setPendingStatus] = useState<ConstructionStatus>("current");
  const [page, setPage] = useState(1);
  const { data, isFetching, error } = useListQuery({ path: "construction-objects", page });
  const [create, createState] = useCreateMutation();

  useEffect(() => {
    if (autoOpen || location.pathname.endsWith("/new")) {
      setOpen(true);
    }
  }, [autoOpen, location.pathname]);

  const objects = useMemo<ConstructionObjectCard[]>(() => {
    const base = (data?.items ?? []) as Array<{
      id: number;
      name: string;
      address: string;
      floorsCount: number;
      sectionsCount: number;
      taktsCount: number;
      startDate?: string;
      endDate?: string;
    }>;

    return base
      .map((item) => {
        const startDate = item.startDate;
        const endDate = item.endDate;
        const status = buildStatus(startDate, endDate);

        return {
          id: item.id,
          name: item.name,
          address: item.address,
          floorsCount: item.floorsCount,
          sectionsCount: item.sectionsCount,
          taktsCount: item.taktsCount,
          startDate,
          endDate,
          status,
        };
      })
      .sort((a, b) => {
        const rank = statusOrder.indexOf(a.status ?? "current") - statusOrder.indexOf(b.status ?? "current");
        if (rank !== 0) return rank;
        return a.name.localeCompare(b.name, "ru");
      });
  }, [data?.items]);

  const closeDrawer = () => {
    setOpen(false);
    if (location.pathname.endsWith("/new")) navigate("/construction-objects", { replace: true });
  };

  const handleCreate = async () => {
    try {
      const values = await form.validateFields();
      const fields = {
        code: values.code ?? `OBJ-${Date.now()}`,
        name: values.name,
        address: values.address,
        floorsCount: values.floorsCount ?? 0,
        sectionsCount: values.sectionsCount ?? 0,
        taktsCount: values.taktsCount ?? 0,
        startDate: values.startDate ? dayjs(values.startDate).format("YYYY-MM-DD") : undefined,
        endDate: values.endDate ? dayjs(values.endDate).format("YYYY-MM-DD") : undefined,
        autoAddFloors: !!values.autoAddFloors,
      };

      await create({ path: "construction-objects", body: fields }).unwrap();
      message.success("Объект добавлен");
      closeDrawer();
      form.resetFields();
      setPendingStatus("current");
    } catch {
      message.error("Проверьте обязательные поля");
    }
  };

  return (
    <>
      <Space
        direction="vertical"
        size={16}
        style={{ width: "100%" }}
      >
        <Space wrap style={{ width: "100%", justifyContent: "space-between" }}>
          <div>
            <Typography.Title level={3} style={{ margin: 0 }}>
              Строительные объекты
            </Typography.Title>
            <Typography.Text type="secondary">
              Текущие, планируемые и завершённые объекты
            </Typography.Text>
          </div>
          <Button
            type="primary"
            icon={<PlusOutlined />}
            onClick={() => navigate("/construction-objects/new")}
          >
            Добавить объект
          </Button>
        </Space>

        {error && <Alert type="error" title="Не удалось загрузить объекты" />}
        {isFetching && <Typography.Text>Загрузка...</Typography.Text>}
        <Row gutter={[16, 16]}>
          {objects.map((item) => {
            const meta = getStatusMeta(item.status ?? "current");
            return (
              <Col xs={24} md={12} xl={8} key={item.id}>
                <Card
                  hoverable
                  onClick={() => navigate(`/construction-objects/${item.id}`)}
                  style={{ height: "100%" }}
                  bodyStyle={{ display: "flex", flexDirection: "column", gap: 12 }}
                >
                  <Space direction="vertical" size={8} style={{ width: "100%" }}>
                    <Space style={{ width: "100%", justifyContent: "space-between" }}>
                      <Typography.Title level={4} style={{ margin: 0 }}>
                        {item.name}
                      </Typography.Title>
                      <Tag color={meta.color}>{meta.title}</Tag>
                    </Space>

                    <Space size={8}>
                      <EnvironmentOutlined />
                      <Typography.Text>{item.address}</Typography.Text>
                    </Space>

                    <Row gutter={[8, 8]}>
                      <Col span={8}>
                        <Typography.Text type="secondary">Этажи</Typography.Text>
                        <div><strong>{item.floorsCount}</strong></div>
                      </Col>
                      <Col span={8}>
                        <Typography.Text type="secondary">Секции</Typography.Text>
                        <div><strong>{item.sectionsCount}</strong></div>
                      </Col>
                      <Col span={8}>
                        <Typography.Text type="secondary">Такты</Typography.Text>
                        <div><strong>{item.taktsCount}</strong></div>
                      </Col>
                    </Row>

                    <Space size={8}>
                      <CalendarOutlined />
                      <Typography.Text>
                        {item.startDate ? dayjs(item.startDate).format("DD.MM.YYYY") : "—"}
                        {" — "}
                        {item.endDate ? dayjs(item.endDate).format("DD.MM.YYYY") : "—"}
                      </Typography.Text>
                    </Space>

                    <Space size={8}>
                      <BuildOutlined />
                      <Typography.Text type="secondary">
                        {item.status === "completed"
                          ? "Строительство завершено"
                          : item.status === "planned"
                            ? "Строительство планируется"
                            : "Строительство ведётся"}
                      </Typography.Text>
                    </Space>
                  </Space>
                </Card>
              </Col>
            );
          })}
        </Row>
        <Pagination current={page} pageSize={20} total={data?.total ?? 0} onChange={setPage} showSizeChanger={false} />
      </Space>

      <Drawer
        title="Новый объект строительства"
        width={480}
        open={open}
        onClose={closeDrawer}
        footer={
          <Space style={{ width: "100%", justifyContent: "flex-end" }}>
            <Button onClick={closeDrawer}>Отмена</Button>
            <Button type="primary" loading={createState.isLoading} onClick={handleCreate}>
              Добавить
            </Button>
          </Space>
        }
      >
        <Form
          form={form}
          layout="vertical"
          initialValues={{
            floorsCount: 6,
            sectionsCount: 2,
            taktsCount: 3,
            autoAddFloors: true,
            startDate: dayjs(),
            endDate: dayjs().add(9, "month"),
          }}
          onValuesChange={(_, all) => {
            if (all.startDate || all.endDate) {
              const nextStatus = buildStatus(
                all.startDate ? dayjs(all.startDate).format("YYYY-MM-DD") : undefined,
                all.endDate ? dayjs(all.endDate).format("YYYY-MM-DD") : undefined,
              );
              setPendingStatus(nextStatus);
            }
          }}
        >
          <Form.Item name="name" label="Название" rules={[{ required: true, message: "Введите название" }]}>
            <Input />
          </Form.Item>

          <Form.Item name="address" label="Адрес" rules={[{ required: true, message: "Введите адрес" }]}>
            <Input />
          </Form.Item>

          <Row gutter={12}>
            <Col span={12}>
              <Form.Item name="floorsCount" label="Этажей в каждой секции">
                <InputNumber min={0} style={{ width: "100%" }} />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="sectionsCount" label="Секций">
                <InputNumber min={0} style={{ width: "100%" }} />
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={12}>
            <Col span={12}>
              <Form.Item name="taktsCount" label="Тактов строительства">
                <InputNumber min={0} style={{ width: "100%" }} />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item label="Статус" style={{ marginBottom: 0 }}>
                <Tag color={getStatusMeta(pendingStatus).color}>
                  {getStatusMeta(pendingStatus).title}
                </Tag>
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={12}>
            <Col span={12}>
              <Form.Item name="startDate" label="Дата начала">
                <DatePicker style={{ width: "100%" }} />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="endDate" label="Дата окончания">
                <DatePicker style={{ width: "100%" }} />
              </Form.Item>
            </Col>
          </Row>

          <Form.Item name="autoAddFloors" valuePropName="checked">
            <Checkbox>Автоматически добавить этажи</Checkbox>
          </Form.Item>
        </Form>
      </Drawer>
    </>
  );
}

export default ConstructionObjectsPage;

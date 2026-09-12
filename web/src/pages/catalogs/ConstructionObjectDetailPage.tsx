import {
  Alert,
  App,
  Button,
  Card,
  Descriptions,
  Space,
  Tabs,
  Tag,
  Typography,
} from "antd";
import { DeleteOutlined, ArrowLeftOutlined } from "@ant-design/icons";
import { useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { useConstructionObjectQuery } from "../../api/endpoints";
import { useObjectStructureQuery } from "../../api/objectWorkspace";
import DeleteConstructionObjectModal from "./DeleteConstructionObjectModal";
import ObjectProductsTable from "./ObjectProductsTable";

function Structure({ objectId }: { objectId: string }) {
  const { data, isLoading, error } = useObjectStructureQuery(objectId);
  if (error) return <Alert type="error" title="Не удалось загрузить структуру объекта" />;
  if (isLoading) return <Typography.Text>Загрузка...</Typography.Text>;
  return (
    <Descriptions
      bordered
      column={1}
      items={data?.sections.map((section) => ({
        key: section.id,
        label: section.name,
        children:
          data.floors
            .filter((f) => f.buildingSectionId === section.id)
            .map((f) => f.name)
            .join(", ") || "Этажи не добавлены",
      }))}
    />
  );
}

export default function ConstructionObjectDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { modal } = App.useApp();
  const [deleting, setDeleting] = useState(false);
  const [editing, setEditing] = useState(false);
  const {
    data: item,
    isLoading,
    error,
  } = useConstructionObjectQuery(id ?? "", { skip: !id });
  const back = () => {
    if (!editing) {
      navigate("/construction-objects");
      return;
    }
    modal.confirm({
      title: "Покинуть страницу?",
      content: "Несохранённые изменения изделия будут потеряны.",
      okText: "Покинуть",
      cancelText: "Остаться",
      onOk: () => navigate("/construction-objects"),
    });
  };
  if (isLoading) return <Typography>Загрузка...</Typography>;
  if (!item || !id)
    return (
      <Space orientation="vertical">
        <Typography.Title level={3}>
          {error && !("status" in error && error.status === 404)
            ? "Не удалось загрузить объект"
            : "Объект не найден"}
        </Typography.Title>
        <Link to="/construction-objects">Назад к объектам</Link>
      </Space>
    );
  return (
    <Space orientation="vertical" size={16} style={{ width: "100%", minWidth: 0 }}>
      <Space wrap style={{ width: "100%", justifyContent: "space-between" }}>
        <Typography.Title level={3} style={{ margin: 0 }}>
          {String(item.name)}
        </Typography.Title>
        <Space>
          <Button icon={<ArrowLeftOutlined />} onClick={back}>
            К объектам
          </Button>
          <Button
            danger
            icon={<DeleteOutlined />}
            disabled={editing}
            onClick={() => setDeleting(true)}
          >
            Удалить объект
          </Button>
        </Space>
      </Space>
      {error && <Alert type="warning" title="Не удалось обновить сведения об объекте" />}
      <Card>
        <Descriptions
          column={{ xs: 1, md: 2, xl: 3 }}
          bordered
          items={[
            { key: "code", label: "Код", children: String(item.code ?? "—") },
            { key: "name", label: "Название", children: String(item.name) },
            { key: "address", label: "Адрес", children: String(item.address ?? "—") },
            { key: "floors", label: "Этажей", children: String(item.floorsCount) },
            { key: "sections", label: "Секций", children: String(item.sectionsCount) },
            { key: "takts", label: "Тактов", children: String(item.taktsCount) },
            {
              key: "start",
              label: "Начало",
              children: item.startDate
                ? new Date(String(item.startDate)).toLocaleDateString("ru-RU")
                : "—",
            },
            {
              key: "end",
              label: "Окончание",
              children: item.endDate
                ? new Date(String(item.endDate)).toLocaleDateString("ru-RU")
                : "—",
            },
            {
              key: "status",
              label: "Статус",
              children: (
                <Tag color={item.isActive ? "blue" : "default"}>
                  {item.isActive ? "Активен" : "Неактивен"}
                </Tag>
              ),
            },
          ]}
        />
      </Card>
      <Card styles={{ body: { minWidth: 0 } }}>
        <Tabs
          defaultActiveKey="products"
          items={[
            {
              key: "products",
              label: "Изделия объекта",
              children: (
                <ObjectProductsTable
                  key={id}
                  objectId={id}
                  readOnly={!item.isActive}
                  onEditingChange={setEditing}
                />
              ),
            },
            {
              key: "structure",
              label: "Секции и этажи",
              disabled: editing,
              children: <Structure objectId={id} />,
            },
          ]}
        />
      </Card>
      {deleting && (
        <DeleteConstructionObjectModal
          objectId={id}
          onCancel={() => setDeleting(false)}
          onDeleted={() => {
            setDeleting(false);
            navigate("/construction-objects", { replace: true });
          }}
        />
      )}
    </Space>
  );
}

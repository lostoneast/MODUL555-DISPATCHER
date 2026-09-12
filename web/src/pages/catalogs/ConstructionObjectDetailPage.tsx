import { Button, Card, Descriptions, Space, Tag, Typography } from "antd";
import { Link, useNavigate, useParams } from "react-router-dom";
import { useConstructionObjectQuery } from "../../api/endpoints";

export default function ConstructionObjectDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { data: item, isFetching, error } = useConstructionObjectQuery(id ?? "", { skip: !id });

  if (error && !("status" in error && error.status === 404))
    return <Typography>Не удалось загрузить объект. Повторите попытку позже.</Typography>;
  if (isFetching) return <Typography>Загрузка...</Typography>;
  if (!item) {
    return (
      <Space direction="vertical">
        <Typography.Title level={3}>Объект не найден</Typography.Title>
        <Link to="/construction-objects">Назад к объектам</Link>
      </Space>
    );
  }

  return (
    <Space direction="vertical" size={16} style={{ width: "100%" }}>
      <Space style={{ width: "100%", justifyContent: "space-between" }}>
        <Typography.Title level={3} style={{ margin: 0 }}>
          {String(item.name)}
        </Typography.Title>
        <Button onClick={() => navigate(-1)}>Назад</Button>
      </Space>

      <Card>
        <Descriptions column={2} bordered>
          <Descriptions.Item label="Название">{String(item.name)}</Descriptions.Item>
          <Descriptions.Item label="Адрес">{String(item.address)}</Descriptions.Item>
          <Descriptions.Item label="Этажей">{String(item.floorsCount)}</Descriptions.Item>
          <Descriptions.Item label="Секций">{String(item.sectionsCount)}</Descriptions.Item>
          <Descriptions.Item label="Тактов">{String(item.taktsCount)}</Descriptions.Item>
          <Descriptions.Item label="Статус">
            <Tag color={item.isActive ? "blue" : "default"}>{item.isActive ? "Активен" : "Неактивен"}</Tag>
          </Descriptions.Item>
        </Descriptions>
      </Card>
    </Space>
  );
}

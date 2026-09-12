import CatalogPage from "../../components/CatalogPage";

export function TransportRoutesPage() {
  return (
    <CatalogPage
      title="Маршруты"
      path="transport-routes"
      columns={[
        { title: "Завод", dataIndex: "plantName" },
        { title: "Объект", dataIndex: "constructionObjectName" },
        { title: "Км", dataIndex: "distanceKm", width: 90 },
        { title: "Минут", dataIndex: "estimatedTravelMinutes", width: 90 },
        {
          title: "Оборотов/сут",
          dataIndex: "turnoverCoefficientPerDay",
          width: 120,
        },
      ]}
      fields={[
        {
          name: "plantId",
          label: "Завод",
          type: "lookup",
          lookup: "plants",
          required: true,
        },
        {
          name: "constructionObjectId",
          label: "Объект",
          type: "lookup",
          lookup: "construction-objects",
          required: true,
        },
        { name: "distanceKm", label: "Расстояние, км", type: "number" },
        {
          name: "estimatedTravelMinutes",
          label: "Время в пути, мин",
          type: "number",
        },
        {
          name: "turnoverCoefficientPerDay",
          label: "Оборотов в сутки",
          type: "number",
        },
      ]}
    />
  );
}

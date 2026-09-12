import CatalogPage from "../../components/CatalogPage";

export function VehiclesPage() {
  return (
    <CatalogPage
      title="Транспорт"
      path="vehicles"
      columns={[
        { title: "Госномер", dataIndex: "registrationNumber", width: 130 },
        { title: "Марка", dataIndex: "make", width: 120 },
        { title: "Модель", dataIndex: "model", width: 120 },
        { title: "Тип", dataIndex: "vehicleTypeName" },
        { title: "Перевозчик", dataIndex: "carrierName" },
      ]}
      fields={[
        {
          name: "vehicleTypeId",
          label: "Тип ТС",
          type: "lookup",
          lookup: "vehicle-types",
          required: true,
        },
        {
          name: "carrierId",
          label: "Перевозчик",
          type: "lookup",
          lookup: "carriers",
          required: true,
        },
        { name: "make", label: "Марка", required: true },
        { name: "model", label: "Модель", required: true },
        { name: "registrationNumber", label: "Госномер", required: true },
        { name: "notes", label: "Примечания", type: "textarea" },
      ]}
    />
  );
}

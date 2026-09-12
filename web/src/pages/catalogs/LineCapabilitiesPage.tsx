import CatalogPage from "../../components/CatalogPage";

export function LineCapabilitiesPage() {
  return (
    <CatalogPage
      title="Возможности линий"
      path="line-capabilities"
      columns={[
        { title: "Линия", dataIndex: "productionLineName" },
        { title: "Тип изделия", dataIndex: "productTypeName" },
        {
          title: "Мощность, шт/сут",
          dataIndex: "defaultDailyCapacityUnits",
          width: 140,
        },
      ]}
      fields={[
        {
          name: "productionLineId",
          label: "Линия",
          type: "lookup",
          lookup: "production-lines",
          required: true,
        },
        {
          name: "productTypeId",
          label: "Тип изделия",
          type: "lookup",
          lookup: "product-types",
          required: true,
        },
        {
          name: "defaultDailyCapacityUnits",
          label: "Мощность, шт/сут",
          type: "number",
          required: true,
        },
      ]}
    />
  );
}

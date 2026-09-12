import CatalogPage from "../../components/CatalogPage";

export function StorageAreasPage() {
  return (
    <CatalogPage
      title="Склады"
      path="storage-areas"
      columns={[
        { title: "Наименование", dataIndex: "name" },
        { title: "Завод", dataIndex: "plantName", width: 160 },
        { title: "Объект", dataIndex: "constructionObjectName", width: 160 },
        { title: "Ёмкость", dataIndex: "capacityUnits", width: 100 },
      ]}
      fields={[
        { name: "name", label: "Наименование", required: true },
        { name: "plantId", label: "Завод", type: "lookup", lookup: "plants" },
        {
          name: "constructionObjectId",
          label: "Объект",
          type: "lookup",
          lookup: "construction-objects",
        },
        {
          name: "capacityUnits",
          label: "Ёмкость, шт",
          type: "number",
          required: true,
        },
      ]}
    />
  );
}

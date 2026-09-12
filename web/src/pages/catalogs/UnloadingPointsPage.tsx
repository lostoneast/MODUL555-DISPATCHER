import CatalogPage from "../../components/CatalogPage";

export function UnloadingPointsPage() {
  return (
    <CatalogPage
      title="Точки разгрузки"
      path="unloading-points"
      columns={[
        { title: "Объект", dataIndex: "constructionObjectName" },
        { title: "Наименование", dataIndex: "name" },
      ]}
      fields={[
        {
          name: "constructionObjectId",
          label: "Объект",
          type: "lookup",
          lookup: "construction-objects",
          required: true,
        },
        { name: "name", label: "Наименование", required: true },
        { name: "description", label: "Описание", type: "textarea" },
      ]}
    />
  );
}

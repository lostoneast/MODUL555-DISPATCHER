import CatalogPage from "../../components/CatalogPage";
import { codeName } from "./shared";

export function BuildingSectionsPage() {
  return (
    <CatalogPage
      title="Секции"
      path="building-sections"
      deletable={false}
      columns={[
        { title: "Объект", dataIndex: "constructionObjectName" },
        ...codeName,
        { title: "Порядок", dataIndex: "sortOrder", width: 90 },
      ]}
      fields={[
        {
          name: "constructionObjectId",
          label: "Объект",
          type: "lookup",
          lookup: "construction-objects",
          required: true,
        },
        { name: "code", label: "Код", required: true },
        { name: "name", label: "Наименование", required: true },
        { name: "sortOrder", label: "Порядок", type: "number" },
      ]}
    />
  );
}

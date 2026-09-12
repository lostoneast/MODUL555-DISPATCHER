import CatalogPage from "../../components/CatalogPage";
import { codeName } from "./shared";

export function ProductTypesPage() {
  return (
    <CatalogPage
      title="Типы изделий"
      path="product-types"
      columns={[...codeName, { title: "Описание", dataIndex: "description" }]}
      fields={[
        { name: "code", label: "Код", required: true },
        { name: "name", label: "Наименование", required: true },
        { name: "description", label: "Описание", type: "textarea" },
      ]}
    />
  );
}

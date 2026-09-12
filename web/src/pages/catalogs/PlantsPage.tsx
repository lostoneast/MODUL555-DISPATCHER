import CatalogPage from "../../components/CatalogPage";
import { codeName } from "./shared";

export function PlantsPage() {
  return (
    <CatalogPage
      title="Заводы"
      path="plants"
      columns={[...codeName, { title: "Адрес", dataIndex: "address" }]}
      fields={[
        { name: "code", label: "Код", required: true },
        { name: "name", label: "Наименование", required: true },
        { name: "address", label: "Адрес", required: true },
      ]}
    />
  );
}

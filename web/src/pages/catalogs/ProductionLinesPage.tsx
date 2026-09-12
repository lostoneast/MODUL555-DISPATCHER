import CatalogPage from "../../components/CatalogPage";
import { codeName } from "./shared";

export function ProductionLinesPage() {
  return (
    <CatalogPage
      title="Производственные линии"
      path="production-lines"
      columns={[
        { title: "Завод", dataIndex: "plantName", width: 180 },
        ...codeName,
      ]}
      fields={[
        {
          name: "plantId",
          label: "Завод",
          type: "lookup",
          lookup: "plants",
          required: true,
        },
        { name: "code", label: "Код", required: true },
        { name: "name", label: "Наименование", required: true },
      ]}
    />
  );
}

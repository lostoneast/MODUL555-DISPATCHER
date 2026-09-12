import CatalogPage from "../../components/CatalogPage";
import { codeName } from "./shared";

export function ConstructionTaktsPage() {
  return (
    <CatalogPage
      title="Такты"
      path="construction-takts"
      deletable={false}
      drawerWidth={520}
      columns={[
        { title: "Объект", dataIndex: "constructionObjectName", width: 160 },
        ...codeName,
        { title: "Порядок", dataIndex: "sequence", width: 90 },
        { title: "Начало", dataIndex: "plannedProductionStartDate", width: 120 },
        { title: "Конец", dataIndex: "plannedProductionEndDate", width: 120 },
        { title: "Статус", dataIndex: "status", width: 100 },
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
        { name: "sequence", label: "Порядок", type: "number", required: true },
        {
          name: "plannedProductionStartDate",
          label: "Начало производства",
          type: "date",
          required: true,
        },
        {
          name: "plannedProductionEndDate",
          label: "Конец производства",
          type: "date",
          required: true,
        },
        {
          name: "status",
          label: "Статус",
          type: "select",
          options: [
            { value: "Draft", label: "Draft" },
            { value: "Planned", label: "Planned" },
            { value: "Active", label: "Active" },
            { value: "Completed", label: "Completed" },
            { value: "Cancelled", label: "Cancelled" },
          ],
        },
        { name: "comment", label: "Комментарий", type: "textarea" },
      ]}
    />
  );
}

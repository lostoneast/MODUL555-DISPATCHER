import CatalogPage from "../../components/CatalogPage";

export function FloorsPage() {
  return (
    <CatalogPage
      title="Этажи"
      path="floors"
      deletable={false}
      columns={[
        { title: "Секция", dataIndex: "buildingSectionName" },
        { title: "№", dataIndex: "number", width: 70 },
        { title: "Наименование", dataIndex: "name" },
        { title: "Порядок", dataIndex: "sortOrder", width: 90 },
      ]}
      fields={[
        {
          name: "buildingSectionId",
          label: "Секция",
          type: "lookup",
          lookup: "building-sections",
          required: true,
        },
        { name: "number", label: "Номер", type: "number" },
        { name: "name", label: "Наименование", required: true },
        { name: "sortOrder", label: "Порядок", type: "number" },
      ]}
    />
  );
}

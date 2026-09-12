import CatalogPage from "../../components/CatalogPage";

export function CarriersPage() {
  return (
    <CatalogPage
      title="Перевозчики"
      path="carriers"
      columns={[
        { title: "Наименование", dataIndex: "name" },
        { title: "Контакты", dataIndex: "contactInfo" },
      ]}
      fields={[
        { name: "name", label: "Наименование", required: true },
        { name: "contactInfo", label: "Контакты", type: "textarea" },
      ]}
    />
  );
}

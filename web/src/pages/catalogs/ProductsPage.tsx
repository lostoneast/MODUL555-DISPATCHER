import CatalogPage from "../../components/CatalogPage";

export function ProductsPage() {
  return (
    <CatalogPage
      title="Изделия"
      path="products"
      showInactive={false}
      deletable={false}
      drawerWidth={560}
      columns={[
        { title: "Код", dataIndex: "productCode", width: 140 },
        { title: "Тип", dataIndex: "productTypeName", width: 160 },
        { title: "Марка", dataIndex: "mark", width: 120 },
        { title: "Ширина", dataIndex: "widthMm", width: 90 },
        { title: "Высота", dataIndex: "heightMm", width: 90 },
        { title: "Толщина", dataIndex: "thicknessMm", width: 90 },
        { title: "Статус", dataIndex: "status", width: 120 },
      ]}
      fields={[
        { name: "productCode", label: "Код изделия", required: true },
        {
          name: "productTypeId",
          label: "Тип изделия",
          type: "lookup",
          lookup: "product-types",
          required: true,
        },
        { name: "mark", label: "Марка", required: true },
        { name: "widthMm", label: "Ширина, мм", type: "number", required: true },
        { name: "heightMm", label: "Высота, мм", type: "number", required: true },
        {
          name: "thicknessMm",
          label: "Толщина, мм",
          type: "number",
          required: true,
        },
        {
          name: "corniceWidthIncreaseMm",
          label: "Припуск карниза, мм",
          type: "number",
        },
        {
          name: "totalWidthWithCorniceMm",
          label: "Ширина с карнизом, мм",
          type: "number",
        },
        {
          name: "thicknessIncreaseMm",
          label: "Припуск толщины, мм",
          type: "number",
        },
        { name: "rightBendMm", label: "Правый загиб, мм", type: "number" },
        { name: "leftBendMm", label: "Левый загиб, мм", type: "number" },
        {
          name: "claddingWidthWithBendsMm",
          label: "Ширина обшивки с загибами, мм",
          type: "number",
        },
        { name: "weightKg", label: "Масса, кг", type: "number" },
        {
          name: "status",
          label: "Статус",
          type: "select",
          options: [
            { value: "Created", label: "Created" },
            { value: "InProduction", label: "InProduction" },
            { value: "InStorage", label: "InStorage" },
            { value: "AssignedToTrip", label: "AssignedToTrip" },
            { value: "InTransit", label: "InTransit" },
            { value: "Delivered", label: "Delivered" },
            { value: "Cancelled", label: "Cancelled" },
          ],
        },
        { name: "additionalInfo", label: "Доп. сведения", type: "textarea" },
      ]}
    />
  );
}

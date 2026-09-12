import CatalogPage from "../../components/CatalogPage";

export function VehicleTypesPage() {
  return (
    <CatalogPage
      title="Типы ТС"
      path="vehicle-types"
      deletable={false}
      drawerWidth={560}
      columns={[
        { title: "Наименование", dataIndex: "name" },
        { title: "Тип ПС", dataIndex: "rollingStockType", width: 140 },
        { title: "Макс. нагрузка", dataIndex: "maxPayloadKg", width: 120 },
        { title: "Площадок", dataIndex: "loadingPlatformCount", width: 100 },
      ]}
      fields={[
        { name: "name", label: "Наименование", required: true },
        { name: "rollingStockType", label: "Тип подвижного состава" },
        { name: "minPayloadKg", label: "Мин. нагрузка, кг", type: "number" },
        { name: "maxPayloadKg", label: "Макс. нагрузка, кг", type: "number" },
        {
          name: "loadingPlatformCount",
          label: "Кол-во площадок",
          type: "number",
          required: true,
        },
        {
          name: "loadingPlatformLengthMm",
          label: "Длина площадки, мм",
          type: "number",
        },
        {
          name: "loadingPlatformWidthMm",
          label: "Ширина площадки, мм",
          type: "number",
        },
        {
          name: "allowedRightLeftImbalanceKg",
          label: "Допустимый перевес, кг",
          type: "number",
        },
        {
          name: "maxCargoHeightMm",
          label: "Макс. высота груза, мм",
          type: "number",
        },
        { name: "cargoVolumeM3", label: "Объём, м³", type: "number" },
        {
          name: "totalTrainLengthMm",
          label: "Длина автопоезда, мм",
          type: "number",
        },
        {
          name: "turningRadiusMm",
          label: "Радиус разворота, мм",
          type: "number",
        },
        { name: "notes", label: "Примечания", type: "textarea" },
      ]}
    />
  );
}

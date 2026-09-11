import CatalogPage from "../components/CatalogPage";

const codeName = [
  { title: "Код", dataIndex: "code", width: 140 },
  { title: "Наименование", dataIndex: "name" },
];

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

export function PlantsPage() {
  return (
    <CatalogPage
      title="Заводы"
      path="plants"
      columns={[
        ...codeName,
        { title: "Адрес", dataIndex: "address" },
      ]}
      fields={[
        { name: "code", label: "Код", required: true },
        { name: "name", label: "Наименование", required: true },
        { name: "address", label: "Адрес", required: true },
      ]}
    />
  );
}

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

export function LineCapabilitiesPage() {
  return (
    <CatalogPage
      title="Возможности линий"
      path="line-capabilities"
      columns={[
        { title: "Линия", dataIndex: "productionLineName" },
        { title: "Тип изделия", dataIndex: "productTypeName" },
        {
          title: "Мощность, шт/сут",
          dataIndex: "defaultDailyCapacityUnits",
          width: 140,
        },
      ]}
      fields={[
        {
          name: "productionLineId",
          label: "Линия",
          type: "lookup",
          lookup: "production-lines",
          required: true,
        },
        {
          name: "productTypeId",
          label: "Тип изделия",
          type: "lookup",
          lookup: "product-types",
          required: true,
        },
        {
          name: "defaultDailyCapacityUnits",
          label: "Мощность, шт/сут",
          type: "number",
          required: true,
        },
      ]}
    />
  );
}

export function ConstructionObjectsPage() {
  return (
    <CatalogPage
      title="Строительные объекты"
      path="construction-objects"
      columns={[...codeName, { title: "Адрес", dataIndex: "address" }]}
      fields={[
        { name: "code", label: "Код", required: true },
        { name: "name", label: "Наименование", required: true },
        { name: "address", label: "Адрес", required: true },
      ]}
    />
  );
}

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

export function StorageAreasPage() {
  return (
    <CatalogPage
      title="Склады"
      path="storage-areas"
      columns={[
        { title: "Наименование", dataIndex: "name" },
        { title: "Завод", dataIndex: "plantName", width: 160 },
        { title: "Объект", dataIndex: "constructionObjectName", width: 160 },
        { title: "Ёмкость", dataIndex: "capacityUnits", width: 100 },
      ]}
      fields={[
        { name: "name", label: "Наименование", required: true },
        { name: "plantId", label: "Завод", type: "lookup", lookup: "plants" },
        {
          name: "constructionObjectId",
          label: "Объект",
          type: "lookup",
          lookup: "construction-objects",
        },
        {
          name: "capacityUnits",
          label: "Ёмкость, шт",
          type: "number",
          required: true,
        },
      ]}
    />
  );
}

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

export function VehiclesPage() {
  return (
    <CatalogPage
      title="Транспорт"
      path="vehicles"
      columns={[
        { title: "Госномер", dataIndex: "registrationNumber", width: 130 },
        { title: "Марка", dataIndex: "make", width: 120 },
        { title: "Модель", dataIndex: "model", width: 120 },
        { title: "Тип", dataIndex: "vehicleTypeName" },
        { title: "Перевозчик", dataIndex: "carrierName" },
      ]}
      fields={[
        {
          name: "vehicleTypeId",
          label: "Тип ТС",
          type: "lookup",
          lookup: "vehicle-types",
          required: true,
        },
        {
          name: "carrierId",
          label: "Перевозчик",
          type: "lookup",
          lookup: "carriers",
          required: true,
        },
        { name: "make", label: "Марка", required: true },
        { name: "model", label: "Модель", required: true },
        { name: "registrationNumber", label: "Госномер", required: true },
        { name: "notes", label: "Примечания", type: "textarea" },
      ]}
    />
  );
}

export function TransportRoutesPage() {
  return (
    <CatalogPage
      title="Маршруты"
      path="transport-routes"
      columns={[
        { title: "Завод", dataIndex: "plantName" },
        { title: "Объект", dataIndex: "constructionObjectName" },
        { title: "Км", dataIndex: "distanceKm", width: 90 },
        { title: "Минут", dataIndex: "estimatedTravelMinutes", width: 90 },
        {
          title: "Оборотов/сут",
          dataIndex: "turnoverCoefficientPerDay",
          width: 120,
        },
      ]}
      fields={[
        {
          name: "plantId",
          label: "Завод",
          type: "lookup",
          lookup: "plants",
          required: true,
        },
        {
          name: "constructionObjectId",
          label: "Объект",
          type: "lookup",
          lookup: "construction-objects",
          required: true,
        },
        { name: "distanceKm", label: "Расстояние, км", type: "number" },
        {
          name: "estimatedTravelMinutes",
          label: "Время в пути, мин",
          type: "number",
        },
        {
          name: "turnoverCoefficientPerDay",
          label: "Оборотов в сутки",
          type: "number",
        },
      ]}
    />
  );
}

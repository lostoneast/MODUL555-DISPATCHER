export const fieldGroups: { title: string; fields: Record<string, string> }[] = [
  { title: "Изделие", fields: {
    productCode: "Код изделия", mark: "Марка", productTypeName: "Тип изделия", productTypeCode: "Код типа",
    status: "Статус изделия", weightKg: "Масса, кг", additionalInfo: "Примечание",
  } },
  { title: "Назначение", fields: {
    constructionObjectName: "Объект", constructionObjectCode: "Код объекта", buildingSectionName: "Секция",
    floorName: "Этаж", installationNumber: "Монтажный №", constructionTaktName: "Такт", constructionTaktCode: "Код такта",
    projectObjectName: "Проектный объект", bindingSource: "Источник привязки",
  } },
  { title: "Потребность", fields: {
    requiredDeliveryDate: "Дата потребности", earliestDeliveryDate: "Не ранее доставки",
    requiredProductionStartDate: "Начало окна производства", requiredProductionEndDate: "Конец окна производства",
    priorityLevel: "Приоритет", priorityOrder: "Порядок приоритета", demandStatus: "Статус потребности", openDemandCount: "Открытых потребностей",
  } },
  { title: "Производство", fields: {
    plantName: "Завод производства", productionLineName: "Линия", plannedProductionDate: "Плановая дата производства",
    productionStatus: "Статус производства", productionMethod: "Способ назначения",
  } },
  { title: "Склад", fields: {
    storageAreaName: "Текущий склад", storageArrivedAt: "Поступление на склад", lastStorageDepartedAt: "Последнее выбытие",
  } },
  { title: "Рейс", fields: {
    tripNumber: "Номер рейса", tripStatus: "Статус рейса", activeTripCount: "Действующих рейсов", loadingSequence: "Порядок погрузки",
    tripPlantName: "Завод отправления", tripObjectName: "Объект доставки", unloadingPointName: "Точка разгрузки",
    vehicleRegistrationNumber: "Госномер ТС", vehicleMake: "Марка ТС", vehicleModel: "Модель ТС", carrierName: "Перевозчик",
    plannedLoadingAt: "План погрузки", plannedDepartureAt: "План отправления", plannedArrivalAt: "План прибытия",
    actualLoadingAt: "Факт погрузки", actualDepartureAt: "Факт отправления", actualArrivalAt: "Факт прибытия",
  } },
  { title: "Геометрия", fields: {
    widthMm: "Ширина, мм", heightMm: "Высота, мм", thicknessMm: "Толщина, мм",
    corniceWidthIncreaseMm: "Припуск карниза, мм", totalWidthWithCorniceMm: "Ширина с карнизом, мм",
    thicknessIncreaseMm: "Припуск толщины, мм", rightBendMm: "Правый загиб, мм", leftBendMm: "Левый загиб, мм",
    claddingWidthWithBendsMm: "Обшивка с загибами, мм",
  } },
  { title: "Идентификаторы", fields: {
    "identifiers.value": "Значения", "identifiers.type": "Типы идентификаторов", "identifiers.isActive": "Активность идентификаторов",
    "identifiers.assignedAt": "Назначение идентификаторов", "identifiers.revokedAt": "Отзыв идентификаторов", "identifiers.id": "ID идентификаторов",
  } },
  { title: "Системные данные", fields: {
    createdAt: "Создано", updatedAt: "Изменено", version: "Версия", id: "ID изделия", productTypeId: "ID типа",
    constructionObjectId: "ID объекта", projectObjectId: "ID проектного объекта", buildingSectionId: "ID секции", floorId: "ID этажа",
    constructionTaktId: "ID такта", demandId: "ID потребности", demandRevisionId: "ID версии потребности", demandTaktId: "ID такта потребности",
    productionAssignmentId: "ID производственного назначения", productionDemandRevisionId: "ID версии производственной потребности",
    plantId: "ID завода", productionLineId: "ID линии", storagePlacementId: "ID размещения", storageAreaId: "ID склада",
    tripId: "ID рейса", tripPlantId: "ID завода рейса", tripObjectId: "ID объекта рейса", unloadingPointId: "ID точки разгрузки",
    vehicleId: "ID транспорта", carrierId: "ID перевозчика",
  } },
];
export const fieldLabels: Record<string, string> = Object.assign({}, ...fieldGroups.map((g) => g.fields));
export const defaultColumns = ["productCode", "mark", "productTypeName", "status", "constructionObjectName", "installationNumber",
  "requiredDeliveryDate", "priorityLevel", "plannedProductionDate", "storageAreaName", "tripNumber", "vehicleRegistrationNumber"];
export const operatorLabels: Record<string, string> = {
  Eq: "Равно", NotEq: "Не равно", Contains: "Содержит", Gte: "От / не меньше", Lte: "До / не больше",
  IsEmpty: "Не заполнено", IsNotEmpty: "Заполнено",
};
export const valueLabels: Record<string, string> = {
  Created: "Создано", InProduction: "В производстве", InStorage: "На складе", AssignedToTrip: "Назначено в рейс",
  InTransit: "В пути", Delivered: "Доставлено", Cancelled: "Отменено", Draft: "Черновик", Active: "Активна",
  Scheduled: "В плане", Fulfilled: "Выполнена", Proposed: "Предложено", Assigned: "Назначено", CapacityConflict: "Превышение мощности",
  Confirmed: "Подтверждено", Completed: "Завершено", Automatic: "Автоматически", Manual: "Вручную", ManualOverride: "Ручная корректировка",
  Planned: "Запланирован", Loading: "Погрузка", Loaded: "Погружен", Arrived: "Прибыл", Unloading: "Разгрузка",
  Low: "Низкий", Normal: "Обычный", High: "Высокий", Urgent: "Срочный", Critical: "Критический",
  InternalCode: "Внутренний код", SerialNumber: "Серийный номер", QrCode: "QR-код", Barcode: "Штрихкод", Rfid: "RFID", Erp: "ERP",
  assignment: "Текущее назначение", project: "По проекту", true: "Да", false: "Нет",
};

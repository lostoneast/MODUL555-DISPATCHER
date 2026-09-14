import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { Alert, App, Button, Card, Col, Descriptions, Form, Input, InputNumber, Row, Select, Space, Spin, Table, Tabs, Typography } from "antd";
import { ArrowLeftOutlined, SaveOutlined } from "@ant-design/icons";
import dayjs from "dayjs";
import { useLookupQuery } from "../../api/endpoints";
import { dimensions, errorMessage, useObjectStructureQuery } from "../../api/objectWorkspace";
import { useProductAvailableTripsQuery, useProductDetailQuery, useSaveProductDetailMutation } from "../../api/productDetail";
import type { ProductDetail, ProductDetailWrite } from "../../api/productDetail";
import { useProductFieldsQuery } from "../../api/productList";
import { fieldGroups, fieldLabels, valueLabels } from "./productTableSchema";
import ProductIdentifiersPanel from "./ProductIdentifiersPanel";
import ProductDemandsPanel from "./ProductDemandsPanel";

function ProductEditor({ id, detail, refreshing, onRefresh }: { id: string; detail: ProductDetail; refreshing: boolean; onRefresh: () => Promise<void> }) {
  const { product, history } = detail;
  const { message, modal } = App.useApp();
  const [form] = Form.useForm<ProductDetailWrite>();
  const [dirty, setDirty] = useState(false);
  const [saveError, setSaveError] = useState("");
  const [save, saving] = useSaveProductDetailMutation();
  const objectId = Form.useWatch("constructionObjectId", form) as number | null | undefined;
  const sectionId = Form.useWatch("buildingSectionId", form) as number | null | undefined;
  const tripId = Form.useWatch("tripId", form) as number | null | undefined;
  const objects = useLookupQuery("construction-objects");
  const types = useLookupQuery("product-types");
  const structure = useObjectStructureQuery(String(objectId), { skip: !objectId });
  const trips = useProductAvailableTripsQuery({ id, objectId }, { skip: !objectId });
  const metadata = useProductFieldsQuery();
  const reset = () => {
    form.setFieldsValue({ ...product } as unknown as ProductDetailWrite);
    setDirty(false); setSaveError("");
  };
  // Keep unsaved input if a background refetch returns a newer server version.
  useEffect(() => {
    if (!dirty && Number(product.version) >= Number(form.getFieldValue("version") ?? 0))
      form.setFieldsValue({ ...product } as unknown as ProductDetailWrite);
  }, [product, form, dirty]);
  useEffect(() => {
    if (!dirty) return;
    const warn = (event: BeforeUnloadEvent) => { event.preventDefault(); event.returnValue = ""; };
    window.addEventListener("beforeunload", warn);
    return () => window.removeEventListener("beforeunload", warn);
  }, [dirty]);
  const activeTripOptions = (trips.currentData ?? []).map((trip) => ({ value: trip.id,
    label: `${trip.tripNumber} · ${trip.plantName} · ${trip.vehicle} · ${trip.plannedArrivalAt ? dayjs(trip.plannedArrivalAt).format("DD.MM.YYYY HH:mm") : "дата не назначена"}` }));
  if (product.tripId && objectId === product.constructionObjectId && !activeTripOptions.some((trip) => trip.value === product.tripId)) {
    activeTripOptions.unshift({ value: Number(product.tripId), label: `${product.tripNumber} · текущее назначение (${valueLabels[String(product.tripStatus)] ?? product.tripStatus})` });
  }
  const tripLocked = !!product.tripId && (!["Draft", "Planned"].includes(String(product.tripStatus)) || !!product.actualLoadingAt || !!product.actualDepartureAt || !!product.actualArrivalAt);
  const physicalLocked = ["InTransit", "Delivered", "Cancelled"].includes(product.status);
  const lookupOptions = (data: { id: string; name: string }[] | undefined, currentId: unknown, name: unknown) => {
    const options = (data ?? []).map((item) => ({ value: Number(item.id), label: item.name }));
    if (currentId && !options.some((item) => item.value === currentId)) options.unshift({ value: Number(currentId), label: String(name ?? currentId) });
    return options;
  };
  const summary = (titles: string[]) => <Space orientation="vertical" style={{ width: "100%" }}>
    {metadata.isError && <Alert type="error" title="Не удалось загрузить перечень полей" action={<Button onClick={() => metadata.refetch()}>Повторить</Button>} />}
    {fieldGroups.filter((group) => titles.includes(group.title)).map((group) => <Descriptions key={group.title} title={group.title} bordered size="small" column={{ xs: 1, sm: 1, md: 2 }}
      items={(metadata.data ?? []).filter((field) => field.field in group.fields).map((field) => {
        const value = product[field.field];
        const text = value == null || value === "" ? "—" : field.type === "datetime" || field.type === "date"
          ? dayjs(String(value)).format(field.type === "date" ? "DD.MM.YYYY" : "DD.MM.YYYY HH:mm")
          : field.type === "enum" || field.type === "boolean" || field.field === "bindingSource" ? valueLabels[String(value)] ?? String(value) : String(value);
        return { key: field.field, label: fieldLabels[field.field], children: text };
      })} />)}
  </Space>;

  return <Space orientation="vertical" size={16} style={{ width: "100%" }}>
    <Space wrap>
      <Link to="/products" onClick={(event) => {
        if (dirty && !window.confirm("Изменения не сохранены. Вернуться к списку?")) event.preventDefault();
      }}><Button icon={<ArrowLeftOutlined />}>К изделиям</Button></Link>
      <Typography.Title level={2} style={{ margin: 0 }}>{product.productCode} · {product.mark}</Typography.Title>
      <Typography.Text type="secondary">{valueLabels[product.status] ?? product.status}</Typography.Text>
    </Space>
    <Form component="div" form={form} layout="vertical" disabled={saving.isLoading} initialValues={product} onValuesChange={() => setDirty(true)}
      onFinish={async (values) => {
        setSaveError("");
        const body = { ...values, productCode: product.productCode, status: product.status,
          version: form.getFieldValue("version"), constructionObjectId: values.constructionObjectId ?? null,
          buildingSectionId: values.buildingSectionId ?? null, floorId: values.floorId ?? null,
          installationNumber: values.installationNumber?.trim() || null, tripId: values.tripId ?? null,
          weightKg: values.weightKg ?? null, additionalInfo: values.additionalInfo?.trim() || null } as ProductDetailWrite;
        try {
          const result = await save({ id, body }).unwrap();
          form.setFieldsValue(result.product as unknown as ProductDetailWrite);
          setDirty(false); message.success("Изделие сохранено");
        } catch (error) { setSaveError(errorMessage(error)); }
      }}>
      <Form.Item name="version" hidden><InputNumber /></Form.Item>
      <Space wrap style={{ marginBottom: 16 }}>
        <Button type="primary" onClick={() => form.submit()} icon={<SaveOutlined />} loading={saving.isLoading} disabled={!dirty}>Сохранить изменения</Button>
        <Button disabled={!dirty || saving.isLoading} onClick={() => modal.confirm({ title: "Отменить несохранённые изменения?", okText: "Отменить изменения", cancelText: "Продолжить редактирование", onOk: reset })}>Отменить изменения</Button>
        {dirty && <Typography.Text type="warning">Есть несохранённые изменения</Typography.Text>}
      </Space>
      {saveError && <Alert type="error" title="Не удалось сохранить изделие" description={saveError} style={{ marginBottom: 16 }} />}
      {dirty && <Typography.Paragraph type="secondary">Перед изменением идентификаторов или потребностей сохраните либо отмените изменения характеристик.</Typography.Paragraph>}
      <Tabs items={[
        { key: "edit", label: "Характеристики и назначение", forceRender: true, children: <Row gutter={[16, 16]}>
          <Col xs={24} xl={12}><Card title="Характеристики">
            <Form.Item label="Код изделия"><Input value={product.productCode} readOnly /></Form.Item>
            <Form.Item name="mark" label="Марка" rules={[{ required: true, whitespace: true, max: 128 }]}><Input maxLength={128} /></Form.Item>
            <Form.Item name="productTypeId" label="Тип изделия" rules={[{ required: true }]}><Select showSearch optionFilterProp="label" loading={types.isFetching}
              options={lookupOptions(types.data, product.productTypeId, product.productTypeName)} /></Form.Item>
            {types.isError && <Alert type="error" title="Типы изделий не загружены" action={<Button onClick={() => types.refetch()}>Повторить</Button>} />}
            <Row gutter={12}>{dimensions.map(([name, label], index) => <Col xs={24} sm={12} key={name}>
              <Form.Item name={name} label={label} rules={[{ required: true }, { type: "number", min: index < 3 ? 0.001 : 0 }]}><InputNumber min={0} style={{ width: "100%" }} /></Form.Item>
            </Col>)}</Row>
            <Form.Item name="weightKg" label="Масса, кг"><InputNumber min={0} style={{ width: "100%" }} /></Form.Item>
            <Form.Item name="additionalInfo" label="Примечание"><Input.TextArea rows={4} maxLength={2000} showCount /></Form.Item>
          </Card></Col>
          <Col xs={24} xl={12}><Space orientation="vertical" style={{ width: "100%" }}>
            <Card title="Расположение изделия">
              <Form.Item name="constructionObjectId" label="Объект"><Select showSearch optionFilterProp="label" loading={objects.isFetching} disabled={physicalLocked || tripLocked}
                options={lookupOptions(objects.data, product.constructionObjectId, product.constructionObjectName)}
                onChange={() => { form.setFieldsValue({ buildingSectionId: null, floorId: null, tripId: null }); }} /></Form.Item>
              {objects.isError && <Alert type="error" title="Объекты не загружены" action={<Button onClick={() => objects.refetch()}>Повторить</Button>} />}
              <Form.Item name="buildingSectionId" label="Секция"><Select allowClear loading={structure.isFetching} disabled={!objectId || physicalLocked || tripLocked}
                options={structure.currentData?.sections.map((section) => ({ value: section.id, label: section.name }))}
                onChange={() => form.setFieldValue("floorId", null)} /></Form.Item>
              <Form.Item name="floorId" label="Этаж"><Select allowClear loading={structure.isFetching} disabled={!sectionId || physicalLocked || tripLocked}
                options={structure.currentData?.floors.filter((floor) => floor.buildingSectionId === sectionId).map((floor) => ({ value: floor.id, label: floor.name }))} /></Form.Item>
              {structure.isError && <Alert type="error" title="Структура объекта не загружена" action={<Button onClick={() => structure.refetch()}>Повторить</Button>} />}
              <Form.Item name="installationNumber" label="Монтажный номер"><Input maxLength={64} disabled={physicalLocked || tripLocked} /></Form.Item>
              <Typography.Text type="secondary">Смена объекта сохраняет историю назначений. Действующие такты и потребности необходимо предварительно переназначить.</Typography.Text>
            </Card>
            <Card title="Назначение в рейс">
              <Form.Item name="tripId" label="Рейс"><Select allowClear showSearch optionFilterProp="label" loading={trips.isFetching}
                disabled={!objectId || physicalLocked || tripLocked} options={activeTripOptions} placeholder="Рейс не назначен"
                notFoundContent={trips.isFetching ? <Spin size="small" /> : "Нет черновых или плановых рейсов на этот объект"} /></Form.Item>
              {trips.isError && <Alert type="error" title="Рейсы не загружены" action={<Button onClick={() => trips.refetch()}>Повторить</Button>} />}
              {tripLocked && <Alert type="info" title="Состав рейса зафиксирован" description="Менять назначение можно до подтверждения рейса и начала погрузки." />}
              {tripId !== product.tripId && <Typography.Paragraph type="secondary">Назначение изменится после сохранения карточки.</Typography.Paragraph>}
              <Typography.Text type="secondary">При сохранении проверяются объект доставки, завод и грузоподъёмность. Назначение рейса не означает фактическую погрузку или производство.</Typography.Text>
            </Card>
          </Space></Col>
        </Row> },
        { key: "demands", label: "Потребности", children: <ProductDemandsPanel id={id} version={Number(product.version)} productStatus={product.status}
          hasObject={!!product.constructionObjectId} disabled={dirty || saving.isLoading || refreshing} onRefresh={onRefresh} /> },
        { key: "operations", label: "Производство и назначение", children: summary(["Потребность", "Производство", "Назначение"]) },
        { key: "logistics", label: "Склад и рейс", children: summary(["Склад", "Рейс"]) },
        { key: "identifiers", label: `Идентификаторы (${product.identifiers.length})`, children: <ProductIdentifiersPanel id={id} version={Number(product.version)}
          identifiers={product.identifiers} disabled={dirty || saving.isLoading || refreshing} onRefresh={onRefresh} /> },
        { key: "history", label: "История назначений", children: <Table rowKey={(row) => `${row.kind}:${row.id}`} size="small" dataSource={history} scroll={{ x: 900 }} columns={[
          { title: "Событие", dataIndex: "kind" }, { title: "Начало", dataIndex: "date", render: (v: string) => dayjs(v).format("DD.MM.YYYY HH:mm") },
          { title: "Окончание / прибытие", dataIndex: "endDate", render: (v: string | null) => v ? dayjs(v).format("DD.MM.YYYY HH:mm") : "—" },
          { title: "Сведения", dataIndex: "description" }, { title: "Примечание", dataIndex: "comment" },
        ]} /> },
        { key: "system", label: "Системные сведения", children: summary(["Системные данные"]) },
      ]} />
    </Form>
  </Space>;
}

export default function ProductDetailPage() {
  const { id = "" } = useParams();
  const valid = /^\d+$/.test(id) && Number(id) > 0;
  const detail = useProductDetailQuery(id, { skip: !valid });
  if (!valid || detail.isError) return <Space orientation="vertical"><Link to="/products">К изделиям</Link>
    <Alert type="error" title="Не удалось открыть изделие" description={valid ? errorMessage(detail.error) : "Некорректный номер изделия"}
      action={valid && <Button onClick={() => detail.refetch()}>Повторить</Button>} /></Space>;
  if (!detail.currentData) return <Spin />;
  return <ProductEditor key={id} id={id} detail={detail.currentData} refreshing={detail.isFetching} onRefresh={async () => { await detail.refetch(); }} />;
}

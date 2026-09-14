import { useState } from "react";
import { Alert, App, Button, DatePicker, Descriptions, Form, Input, InputNumber, Modal, Select, Space, Table, Typography } from "antd";
import dayjs from "dayjs";
import type { Dayjs } from "dayjs";
import { useCancelProductDemandMutation, useProductDemandsQuery, useProductDemandTaktsQuery, useSaveProductDemandMutation } from "../../api/productOperations";
import type { ProductDemand } from "../../api/productOperations";
import { errorMessage } from "../../api/objectWorkspace";
import { valueLabels } from "./productTableSchema";

type DemandForm = { constructionTaktId: number; earliestDeliveryDate: Dayjs; requiredDeliveryDate: Dayjs;
  priorityLevel: string; priorityOrder: number; status: string; comment?: string; reason: string };
const date = (value: string) => dayjs(value).format("DD.MM.YYYY");

export default function ProductDemandsPanel({ id, version, productStatus, hasObject, disabled, onRefresh }: {
  id: string; version: number; productStatus: string; hasObject: boolean; disabled: boolean; onRefresh: () => Promise<void>;
}) {
  const { message } = App.useApp();
  const [form] = Form.useForm<DemandForm>();
  const [editor, setEditor] = useState<{ demand?: ProductDemand; cancel: boolean; version: number } | null>(null);
  const [error, setError] = useState("");
  const [refreshing, setRefreshing] = useState(false);
  const demands = useProductDemandsQuery(id);
  const takts = useProductDemandTaktsQuery(id);
  const [save, saving] = useSaveProductDemandMutation();
  const [cancel, cancelling] = useCancelProductDemandMutation();
  const busy = saving.isLoading || cancelling.isLoading || refreshing;
  const selectedTaktId = Form.useWatch("constructionTaktId", form);
  const selectedTakt = takts.data?.find((takt) => takt.id === selectedTaktId);
  const closedProduct = ["Delivered", "Cancelled"].includes(productStatus);
  const open = (demand?: ProductDemand, cancel = false) => {
    const revision = demand?.revisions.find((r) => r.id === demand.currentRevisionId);
    form.resetFields(); setError("");
    form.setFieldsValue({ constructionTaktId: revision?.constructionTaktId,
      earliestDeliveryDate: revision ? dayjs(revision.earliestDeliveryDate) : undefined,
      requiredDeliveryDate: revision ? dayjs(revision.requiredDeliveryDate) : undefined,
      priorityLevel: revision?.priorityLevel ?? "Normal", priorityOrder: revision?.priorityOrder ?? 0,
      status: demand?.status === "Draft" ? "Draft" : "Active", comment: revision?.comment ?? "", reason: "" });
    setEditor({ demand, cancel, version });
  };
  return <Space orientation="vertical" size={12} style={{ width: "100%" }}>
    <Typography.Text type="secondary">Изменения сохраняются новой версией. Производственное окно копируется из такта. Потребность с действующим производственным назначением требует пересмотра плана.</Typography.Text>
    {!hasObject && <Alert type="info" title="Сначала назначьте изделию объект и сохраните карточку." />}
    <Button type="primary" disabled={disabled || busy || closedProduct || !hasObject || demands.isFetching} onClick={() => open()}>Добавить потребность</Button>
    {demands.isError && <Alert type="error" title="Не удалось загрузить потребности" description={errorMessage(demands.error)} action={<Button onClick={() => demands.refetch()}>Повторить</Button>} />}
    <Table<ProductDemand> rowKey="id" size="small" dataSource={demands.currentData ?? []} loading={demands.isFetching} scroll={{ x: 1050 }}
      columns={[
        { title: "№", dataIndex: "id" }, { title: "Статус", dataIndex: "status", render: (v: string) => valueLabels[v] ?? v },
        { title: "Такт", render: (_, row) => row.revisions.find((r) => r.id === row.currentRevisionId)?.taktName ?? "—" },
        { title: "Дата потребности", render: (_, row) => { const r = row.revisions.find((r) => r.id === row.currentRevisionId); return r ? date(r.requiredDeliveryDate) : "—"; } },
        { title: "Приоритет", render: (_, row) => { const r = row.revisions.find((r) => r.id === row.currentRevisionId); return r ? `${valueLabels[r.priorityLevel] ?? r.priorityLevel} / ${r.priorityOrder}` : "—"; } },
        { title: "Производственный план", render: (_, row) => row.hasProductionAssignments ? "Есть назначение" : "Не назначен" },
        { title: "Действия", render: (_, row) => !["Fulfilled", "Cancelled"].includes(row.status) && <Space>
          <Button size="small" disabled={disabled || busy || demands.isFetching || closedProduct || row.hasProductionAssignments} onClick={() => open(row)}>Изменить</Button>
          <Button size="small" danger disabled={disabled || busy || demands.isFetching || row.hasProductionAssignments} onClick={() => open(row, true)}>Отменить</Button>
        </Space> },
      ]}
      expandable={{ expandedRowRender: (row) => <Space orientation="vertical" style={{ width: "100%" }}>{row.revisions.map((r) => <Descriptions key={r.id}
        title={`Версия ${r.revisionNumber}${r.id === row.currentRevisionId ? " · текущая" : ""}`} size="small" bordered column={{ xs: 1, sm: 2 }} items={[
          { key: "takt", label: "Такт", children: r.taktName },
          { key: "production", label: "Производственное окно", children: `${date(r.requiredProductionStartDate)} — ${date(r.requiredProductionEndDate)}` },
          { key: "delivery", label: "Доставка", children: `${date(r.earliestDeliveryDate)} — ${date(r.requiredDeliveryDate)}` },
          { key: "priority", label: "Приоритет / порядок", children: `${valueLabels[r.priorityLevel] ?? r.priorityLevel} / ${r.priorityOrder}` },
          { key: "reason", label: "Причина", children: r.changeReason ?? "—" }, { key: "comment", label: "Примечание", children: r.comment ?? "—" },
          { key: "created", label: "Создана", children: dayjs(r.createdAt).format("DD.MM.YYYY HH:mm") }, { key: "author", label: "Автор", children: r.createdByUserId ?? "—" },
        ]} />)}</Space> }} />
    <Modal open={!!editor} width={640} title={editor?.cancel ? "Отменить потребность" : editor?.demand ? "Новая версия потребности" : "Новая потребность"}
      okText={editor?.cancel ? "Отменить потребность" : "Сохранить"} cancelText="Закрыть" maskClosable={false} confirmLoading={busy}
      closable={!busy} cancelButtonProps={{ disabled: busy }} okButtonProps={{ danger: editor?.cancel, disabled: disabled || (!editor?.cancel && (takts.isError || takts.isFetching)) }}
      onCancel={() => { if (!busy) setEditor(null); }} onOk={() => form.submit()}>
      {error && <Alert type="error" title={error} style={{ marginBottom: 12 }} />}
      {editor?.cancel && <Alert type="warning" title="Потребность будет отменена. Её версии сохранятся в истории." style={{ marginBottom: 12 }} />}
      {takts.isError && !editor?.cancel && <Alert type="error" title="Не удалось загрузить такты" action={<Button onClick={() => takts.refetch()}>Повторить</Button>} />}
      <Form form={form} layout="vertical" disabled={busy} onFinish={async (values) => {
        if (!editor) return;
        setError("");
        try {
          if (editor.cancel && editor.demand) await cancel({ id, demandId: editor.demand.id,
            body: { productVersion: editor.version, demandVersion: editor.demand.version, reason: values.reason } }).unwrap();
          else await save({ id, demandId: editor.demand?.id, body: { ...values, productVersion: editor.version, demandVersion: editor.demand?.version,
            earliestDeliveryDate: values.earliestDeliveryDate.format("YYYY-MM-DD"), requiredDeliveryDate: values.requiredDeliveryDate.format("YYYY-MM-DD") } }).unwrap();
        } catch (e) { setError(errorMessage(e)); return; }
        setEditor(null); message.success("Потребность сохранена"); setRefreshing(true);
        try { await onRefresh(); await demands.refetch(); } finally { setRefreshing(false); }
      }}>
        {!editor?.cancel && <>
          <Form.Item name="constructionTaktId" label="Такт текущего объекта" rules={[{ required: true }]}><Select showSearch optionFilterProp="label" loading={takts.isFetching}
            options={takts.data?.map((takt) => ({ value: takt.id, label: takt.name }))} /></Form.Item>
          <Typography.Paragraph type="secondary">Производственное окно: {selectedTakt ? `${date(selectedTakt.productionStartDate)} — ${date(selectedTakt.productionEndDate)}` : "выберите такт"}</Typography.Paragraph>
          <Form.Item name="earliestDeliveryDate" label="Не ранее доставки" rules={[{ required: true }]}><DatePicker format="DD.MM.YYYY" style={{ width: "100%" }} /></Form.Item>
          <Form.Item name="requiredDeliveryDate" label="Дата потребности" dependencies={["earliestDeliveryDate", "constructionTaktId"]} rules={[{ required: true },
            { validator: (_, value: Dayjs | undefined) => !value || ((!form.getFieldValue("earliestDeliveryDate") || !value.isBefore(form.getFieldValue("earliestDeliveryDate"), "day"))
              && (!selectedTakt || !value.isBefore(dayjs(selectedTakt.productionEndDate), "day"))) ? Promise.resolve() : Promise.reject(new Error("Дата должна быть не раньше начала доставки и окончания производства.")) }]}>
            <DatePicker format="DD.MM.YYYY" style={{ width: "100%" }} /></Form.Item>
          <Form.Item name="priorityLevel" label="Приоритет" rules={[{ required: true }]}><Select options={["Low", "Normal", "High", "Urgent", "Critical"].map((value) => ({ value, label: valueLabels[value] }))} /></Form.Item>
          <Form.Item name="priorityOrder" label="Порядок внутри приоритета" rules={[{ required: true }]}><InputNumber min={0} max={2147483647} precision={0} style={{ width: "100%" }} /></Form.Item>
          <Form.Item name="status" label="Статус" rules={[{ required: true }]}><Select options={[{ value: "Draft", label: "Черновик" }, { value: "Active", label: "Активна" }]} /></Form.Item>
          <Form.Item name="comment" label="Примечание"><Input.TextArea maxLength={2000} rows={2} /></Form.Item>
        </>}
        <Form.Item name="reason" label="Причина изменения" rules={[{ required: true, whitespace: true, max: 512 }]}><Input.TextArea maxLength={512} rows={3} /></Form.Item>
      </Form>
    </Modal>
  </Space>;
}

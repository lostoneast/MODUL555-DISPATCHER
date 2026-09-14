import { useState } from "react";
import { Alert, App, Button, Form, Input, Modal, Select, Space, Table, Typography } from "antd";
import dayjs from "dayjs";
import type { ProductIdentifierRow } from "../../api/productList";
import { useProductIdentifierTypesQuery, useRevokeProductIdentifierMutation, useSaveProductIdentifierMutation } from "../../api/productOperations";
import { errorMessage } from "../../api/objectWorkspace";
import { valueLabels } from "./productTableSchema";

export default function ProductIdentifiersPanel({ id, version, identifiers, disabled, onRefresh }: {
  id: string; version: number; identifiers: ProductIdentifierRow[]; disabled: boolean; onRefresh: () => Promise<void>;
}) {
  const { message } = App.useApp();
  const [form] = Form.useForm<{ type: string; value: string; reason: string }>();
  const [editor, setEditor] = useState<{ row?: ProductIdentifierRow; revoke: boolean; version: number } | null>(null);
  const [error, setError] = useState("");
  const [refreshing, setRefreshing] = useState(false);
  const types = useProductIdentifierTypesQuery(id);
  const [save, saving] = useSaveProductIdentifierMutation();
  const [revoke, revoking] = useRevokeProductIdentifierMutation();
  const busy = saving.isLoading || revoking.isLoading || refreshing;
  const open = (row?: ProductIdentifierRow, revoke = false) => {
    setError(""); form.resetFields(); form.setFieldsValue({ type: row?.type, value: row?.value, reason: "" });
    setEditor({ row, revoke, version });
  };
  return <Space orientation="vertical" size={12} style={{ width: "100%" }}>
    <Typography.Text type="secondary">Исправление создаёт новый идентификатор и отзывает прежний. Отозванные значения остаются в истории и не используются повторно.</Typography.Text>
    <Button type="primary" disabled={disabled || busy} onClick={() => open()}>Добавить идентификатор</Button>
    <Table rowKey="id" size="small" dataSource={identifiers} scroll={{ x: 900 }} columns={[
      { title: "Тип", dataIndex: "type", render: (v: string) => valueLabels[v] ?? v },
      { title: "Значение", dataIndex: "value" }, { title: "Активен", dataIndex: "isActive", render: (v: boolean) => v ? "Да" : "Нет" },
      { title: "Назначен", dataIndex: "assignedAt", render: (v: string) => dayjs(v).format("DD.MM.YYYY HH:mm") },
      { title: "Отозван", dataIndex: "revokedAt", render: (v: string | null) => v ? dayjs(v).format("DD.MM.YYYY HH:mm") : "—" },
      { title: "Действия", key: "actions", render: (_, row) => row.isActive && <Space>
        <Button size="small" disabled={disabled || busy} onClick={() => open(row)}>Исправить</Button>
        <Button size="small" danger disabled={disabled || busy} onClick={() => open(row, true)}>Отозвать</Button>
      </Space> },
    ]} />
    <Modal open={!!editor} title={editor?.revoke ? "Отозвать идентификатор" : editor?.row ? "Исправить идентификатор" : "Новый идентификатор"}
      okText={editor?.revoke ? "Отозвать" : "Сохранить"} cancelText="Закрыть" confirmLoading={busy} maskClosable={false}
      closable={!busy} cancelButtonProps={{ disabled: busy }} okButtonProps={{ danger: editor?.revoke, disabled: disabled || (!editor?.revoke && types.isError) }}
      onCancel={() => { if (!busy) setEditor(null); }} onOk={() => form.submit()}>
      {error && <Alert type="error" title={error} style={{ marginBottom: 12 }} />}
      {types.isError && !editor?.revoke && <Alert type="error" title="Не удалось загрузить типы" action={<Button onClick={() => types.refetch()}>Повторить</Button>} />}
      <Form form={form} layout="vertical" disabled={busy} onFinish={async (values) => {
        if (!editor) return;
        setError("");
        try {
          if (editor.revoke && editor.row) await revoke({ id, identifierId: editor.row.id, body: { productVersion: editor.version, reason: values.reason } }).unwrap();
          else await save({ id, identifierId: editor.row?.id, body: { ...values, value: values.value.trim(), productVersion: editor.version } }).unwrap();
        } catch (e) { setError(errorMessage(e)); return; }
        setEditor(null); message.success("Идентификатор сохранён"); setRefreshing(true);
        try { await onRefresh(); } finally { setRefreshing(false); }
      }}>
        <Form.Item name="type" label="Тип" rules={[{ required: true }]}><Select disabled={busy || editor?.revoke} loading={types.isFetching} options={types.data?.map((type) => ({ value: type, label: valueLabels[type] ?? type }))} /></Form.Item>
        <Form.Item name="value" label="Значение" rules={[{ required: true, whitespace: true, max: 256 }]}><Input maxLength={256} disabled={busy || editor?.revoke} /></Form.Item>
        <Form.Item name="reason" label="Причина изменения" rules={[{ required: true, whitespace: true, max: 512 }]}><Input.TextArea maxLength={512} rows={3} /></Form.Item>
      </Form>
    </Modal>
  </Space>;
}

import { Alert, App, Button, Form, Input, InputNumber, Modal, Space, Typography, Upload } from "antd";
import { DownloadOutlined, InboxOutlined } from "@ant-design/icons";
import { useState } from "react";
import { errorMessage, useImportObjectProductsMutation, useObjectImportExampleMutation } from "../../api/objectWorkspace";

export default function ImportObjectProductsModal({ objectId, onClose }: { objectId: string; onClose: () => void }) {
  const { message } = App.useApp();
  const [file, setFile] = useState<File>();
  const [suffix, setSuffix] = useState("");
  const [startNumber, setStartNumber] = useState<number | null>(1);
  const [errors, setErrors] = useState<string[]>([]);
  const [completed, setCompleted] = useState<number>();
  const [upload, { isLoading }] = useImportObjectProductsMutation();
  const [download, { isLoading: downloading }] = useObjectImportExampleMutation();
  const resetResult = () => { setErrors([]); setCompleted(undefined); };
  const submit = async () => {
    if (!file || startNumber == null || !Number.isInteger(startNumber) || startNumber < 1) return;
    resetResult();
    try {
      const result = await upload({ objectId, file, suffix: suffix.trim(), startNumber }).unwrap();
      setErrors(result.errors);
      if (result.errors.length === 0) {
        setCompleted(result.created);
        message.success(`Добавлено изделий: ${result.created}`);
      }
    } catch (error) { setErrors([errorMessage(error)]); }
  };
  const codeExample = ["НС-1", suffix.trim(), String(startNumber ?? 1).padStart(3, "0")].filter(Boolean).join("-");
  return <Modal open title="Импорт изделий в текущий объект" width={680} onCancel={onClose}
    closable={!isLoading} maskClosable={!isLoading} keyboard={!isLoading}
    cancelText="Закрыть" cancelButtonProps={{ disabled: isLoading }}
    okText={completed === undefined ? "Импортировать" : "Готово"} confirmLoading={isLoading}
    onOk={completed === undefined ? submit : onClose}
    okButtonProps={{ disabled: completed === undefined && (!file || startNumber == null || !Number.isInteger(startNumber) || startNumber < 1 || startNumber > 2147483647) }}>
    <Space orientation="vertical" size={16} style={{ width: "100%" }}>
      <Alert type="info" showIcon title="Изделия автоматически прикрепятся к открытому объекту"
        description="Обязательные столбцы: Марка, Ширина, мм, Высота, мм, Толщина, мм. Код и тип можно оставить пустыми. Монтажный номер сохраняется в привязке к объекту. Новые изделия получают статус «Создано»." />
      <Button icon={<DownloadOutlined />} loading={downloading} onClick={() => void download(objectId).unwrap().catch((e) => message.error(errorMessage(e)))}>
        Скачать пример файла импорта
      </Button>
      <Form layout="vertical" disabled={isLoading}>
        <Form.Item label="Суффикс кода изделия" extra="Необязательно. Используется только для строк без готового кода.">
          <Input value={suffix} maxLength={20} placeholder="Например: ОБЪЕКТ" onChange={(e) => { setSuffix(e.target.value); resetResult(); }} />
        </Form.Item>
        <Form.Item label="Начальный номер">
          <InputNumber value={startNumber} min={1} max={2147483647} precision={0} style={{ width: "100%" }}
            onChange={(value) => { setStartNumber(value); resetResult(); }} />
        </Form.Item>
        <Typography.Paragraph>Пример кода: <Typography.Text code>{codeExample}</Typography.Text>.
          Тип для марок НС-1 и НС-2 — <strong>НС</strong>. Занятые номера будут пропущены.</Typography.Paragraph>
      </Form>
      <Upload.Dragger accept=".xlsx" maxCount={1} disabled={isLoading} fileList={file ? [{ uid: "import", name: file.name }] : []}
        beforeUpload={(candidate) => {
          if (!candidate.name.toLowerCase().endsWith(".xlsx") || candidate.size > 19_000_000) {
            message.error("Выберите файл .xlsx размером до 19 МБ."); return Upload.LIST_IGNORE;
          }
          setFile(candidate); resetResult(); return false;
        }} onRemove={() => { setFile(undefined); resetResult(); }}>
        <p className="ant-upload-drag-icon"><InboxOutlined /></p>
        <p className="ant-upload-text">Выберите или перетащите файл Excel</p>
        <p className="ant-upload-hint">Первый лист, до 5000 изделий. Первая заполненная строка — заголовки.</p>
      </Upload.Dragger>
      <Typography.Text type="secondary">Существующие изделия не изменяются. При ошибках не сохраняется ни одна строка файла.
        Повторный импорт без готовых кодов создаст новые изделия.</Typography.Text>
      {errors.length > 0 && <Alert type="error" showIcon title="Импорт не выполнен" description={
        <div style={{ maxHeight: 220, overflowY: "auto" }}>{errors.map((error, index) => <div key={index}>{error}</div>)}</div>} />}
      {completed !== undefined && <Alert type="success" showIcon title={`Добавлено изделий: ${completed}`} description="Таблица объекта обновлена. Если включены фильтры, новые изделия могут быть скрыты." />}
    </Space>
  </Modal>;
}

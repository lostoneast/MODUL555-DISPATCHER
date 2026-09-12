import {
  Alert,
  App,
  Checkbox,
  Descriptions,
  Input,
  Modal,
  Space,
  Spin,
  Typography,
  Button,
} from "antd";
import { useState } from "react";
import {
  errorMessage,
  useDeleteConstructionObjectMutation,
  useObjectDeletionPreviewQuery,
} from "../../api/objectWorkspace";

export default function DeleteConstructionObjectModal({
  objectId,
  onCancel,
  onDeleted,
}: {
  objectId: string;
  onCancel: () => void;
  onDeleted: () => void;
}) {
  const { message } = App.useApp();
  const [name, setName] = useState("");
  const [understood, setUnderstood] = useState(false);
  const [failure, setFailure] = useState<string>();
  const { data, isFetching, error, refetch } = useObjectDeletionPreviewQuery(objectId, {
    refetchOnMountOrArgChange: true,
  });
  const [remove, { isLoading }] = useDeleteConstructionObjectMutation();
  const reload = () => {
    setName("");
    setUnderstood(false);
    setFailure(undefined);
    void refetch();
  };
  const submit = async () => {
    if (!data || name !== data.name || !understood || isFetching) return;
    try {
      await remove({
        id: objectId,
        confirmationName: name,
        previewToken: data.previewToken,
      }).unwrap();
      message.success("Объект и связанные данные удалены");
      onDeleted();
    } catch (e) {
      setFailure(errorMessage(e));
      setName("");
      setUnderstood(false);
      void refetch();
    }
  };
  return (
    <Modal
      open
      title="Удалить объект навсегда?"
      width={660}
      onCancel={onCancel}
      closable={!isLoading}
      maskClosable={!isLoading}
      keyboard={!isLoading}
      okText="Удалить объект и связанные данные"
      cancelText="Отмена"
      onOk={submit}
      confirmLoading={isLoading}
      cancelButtonProps={{ disabled: isLoading }}
      okButtonProps={{
        danger: true,
        disabled: !data || !!error || isFetching || name !== data.name || !understood,
      }}
    >
      <Space orientation="vertical" size={16} style={{ width: "100%" }}>
        <Alert
          type="error"
          showIcon
          title="Действие необратимо"
          description="Объект, его секции, этажи, такты, склады, рейсы и связанные записи будут удалены из базы данных. Восстановить их через приложение нельзя."
        />
        {isFetching && (
          <Spin tip="Рассчитываем связанные данные">
            <div style={{ height: 40 }} />
          </Spin>
        )}
        {error && (
          <Alert
            type="error"
            title="Не удалось рассчитать удаление"
            action={<Button onClick={reload}>Повторить</Button>}
          />
        )}
        {failure && <Alert type="error" title={failure} />}
        {data && !isFetching && (
          <>
            <Typography.Text>
              Объект: <strong>{data.name}</strong>
            </Typography.Text>
            <Alert
              type="warning"
              showIcon
              title={`Изделий будет удалено: ${data.productsToDelete}. Сохранено: ${data.productsToKeep}.`}
              description="Удаляются только изделия «Создано» без признаков производства, хранения, перевозки и без связей с другими объектами. Физические, отменённые и спорные изделия сохраняются. Привязки и история, относящиеся к этому объекту, удаляются. Общие справочники и журнал аудита сохраняются."
            />
            <Descriptions
              size="small"
              bordered
              column={2}
              items={Object.entries(data.records)
                .filter(([, count]) => count > 0)
                .map(([label, count]) => ({ key: label, label, children: count }))}
            />
            <Checkbox
              checked={understood}
              disabled={isLoading}
              onChange={(e) => setUnderstood(e.target.checked)}
            >
              Я понимаю, что указанные данные будут удалены без возможности отмены
            </Checkbox>
            <label htmlFor="delete-object-name">
              Введите вручную точное название объекта: <strong>{data.name}</strong>
            </label>
            <Input
              id="delete-object-name"
              value={name}
              onChange={(e) => setName(e.target.value)}
              disabled={isLoading}
              autoComplete="off"
              placeholder="Название с учётом регистра и пробелов"
              onPaste={(e) => e.preventDefault()}
              onDrop={(e) => e.preventDefault()}
            />
          </>
        )}
      </Space>
    </Modal>
  );
}

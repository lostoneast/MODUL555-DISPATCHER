import { Button, Card, Form, Input, Typography, Alert } from "antd";
import { useNavigate } from "react-router-dom";
import { useDispatch } from "react-redux";
import { useLoginMutation } from "../api/endpoints";
import { credentialsReceived } from "../features/auth/authSlice";
import { Logo } from "../components/Logo";
import { brand } from "../theme";

export default function LoginPage() {
  const [login, { isLoading, error }] = useLoginMutation();
  const dispatch = useDispatch();
  const navigate = useNavigate();

  const errMsg =
    error && "data" in error && error.data && typeof error.data === "object"
      ? String((error.data as { message?: string }).message ?? "Ошибка входа")
      : error
        ? "Ошибка входа"
        : null;

  return (
    <div
      style={{
        position: "fixed",
        inset: 0,
        display: "grid",
        placeItems: "center",
        background: `linear-gradient(135deg, ${brand.deepBlue} 0%, ${brand.skyBlue} 100%)`,
        padding: 24,
        overflow: "auto",
      }}
    >
      <Card
        style={{
          width: 420,
          maxWidth: "100%",
          boxShadow: "0 16px 40px rgba(0,0,0,.18)",
        }}
      >
        <Logo />
        <Typography.Title level={4} style={{ marginTop: 20, color: brand.deepBlue }}>
          Диспетчер производства
        </Typography.Title>
        <Typography.Paragraph type="secondary">
          Вход через Keycloak
        </Typography.Paragraph>
        {errMsg ? (
          <Alert type="error" showIcon message={errMsg} style={{ marginBottom: 16 }} />
        ) : null}
        <Form
          layout="vertical"
          onFinish={async (values: { userName: string; password: string }) => {
            const result = await login(values).unwrap();
            dispatch(
              credentialsReceived({
                token: result.token,
                refreshToken: result.refreshToken,
                user: result.user,
              }),
            );
            navigate("/");
          }}
        >
          <Form.Item
            name="userName"
            label="Логин"
            rules={[{ required: true, message: "Укажите логин" }]}
          >
            <Input autoComplete="username" />
          </Form.Item>
          <Form.Item
            name="password"
            label="Пароль"
            rules={[{ required: true, message: "Укажите пароль" }]}
          >
            <Input.Password autoComplete="current-password" />
          </Form.Item>
          <Button type="primary" htmlType="submit" block loading={isLoading}>
            Войти
          </Button>
        </Form>
      </Card>
    </div>
  );
}

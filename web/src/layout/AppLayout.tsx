import { Layout, Menu, Dropdown, Typography, Badge } from "antd";
import { DashboardOutlined, LogoutOutlined } from "@ant-design/icons";
import { Link, Outlet, useLocation, useNavigate } from "react-router-dom";
import { useDispatch, useSelector } from "react-redux";
import type { MenuProps } from "antd";
import { useMemo } from "react";
import { Logo } from "../components/Logo";
import { loggedOut } from "../features/auth/authSlice";
import type { RootState } from "../store";
import { brand } from "../theme";

export default function AppLayout() {
  const location = useLocation();
  const user = useSelector((s: RootState) => s.auth.user);
  const dispatch = useDispatch();
  const navigate = useNavigate();

  const items: MenuProps["items"] = useMemo(
    () => [
      {
        key: "/",
        icon: <DashboardOutlined />,
        label: <Link to="/">Изделия</Link>,
      },
    ],
    [],
  );

  return (
    <Layout style={{ minHeight: "100vh" }} hasSider>
      <Layout.Sider
        width={268}
        theme="dark"
        breakpoint="lg"
        collapsedWidth={0}
        trigger={null}
        className="app-sider"
        style={{
          background: brand.deepBlue,
          height: "100vh",
          position: "sticky",
          top: 0,
          overflow: "auto",
        }}
      >
        <div style={{ padding: "16px 16px 8px" }}>
          <Logo light markSize={32} />
          <Typography.Text
            style={{
              display: "block",
              color: "#8FC3E4",
              fontSize: 11,
              marginTop: 6,
            }}
          >
            Диспетчер · производство и логистика
          </Typography.Text>
        </div>
        <Menu
          theme="dark"
          mode="inline"
          selectedKeys={[location.pathname]}
          items={items}
          style={{ background: brand.deepBlue, borderInlineEnd: "none" }}
        />
      </Layout.Sider>
      <Layout>
        <Layout.Header
          style={{
            display: "flex",
            alignItems: "center",
            justifyContent: "flex-end",
            gap: 16,
          }}
        >
          <Badge
            color={brand.skyBlue}
            text={<span style={{ color: "#fff" }}>{user?.position}</span>}
          />
          <Dropdown
            menu={{
              items: [
                {
                  key: "out",
                  icon: <LogoutOutlined />,
                  label: "Выйти",
                  onClick: () => {
                    dispatch(loggedOut());
                    navigate("/login");
                  },
                },
              ],
            }}
          >
            <Typography.Text style={{ color: "#fff", cursor: "pointer" }}>
              {user?.fullName}
            </Typography.Text>
          </Dropdown>
        </Layout.Header>
        <Layout.Content style={{ padding: 20 }}>
          <Outlet />
        </Layout.Content>
      </Layout>
    </Layout>
  );
}

import { Layout, Menu, Dropdown, Typography, Badge } from "antd";
import {
  AppstoreOutlined,
  BankOutlined,
  CarOutlined,
  DatabaseOutlined,
  LogoutOutlined,
  BuildOutlined,
  SettingOutlined,
  AuditOutlined,
  TeamOutlined,
} from "@ant-design/icons";
import { Link, Outlet, useLocation, useNavigate } from "react-router-dom";
import { useDispatch, useSelector } from "react-redux";
import type { MenuProps } from "antd";
import { useEffect, useMemo, useState } from "react";
import { Logo } from "../components/Logo";
import { loggedOut } from "../features/auth/authSlice";
import type { RootState } from "../store";
import { brand } from "../theme";

function isAdmin(roles: string[] | undefined) {
  return !!roles?.some((r) => r === "admin" || r === "manager");
}

function sectionForPath(pathname: string): string | null {
  if (pathname.startsWith("/admin")) return "admin";
  if (
    ["/product-types", "/plants", "/production-lines", "/line-capabilities"].some(
      (p) => pathname === p || pathname.startsWith(p + "/"),
    )
  )
    return "production";
  if (
    [
      "/construction-objects/new",
      "/construction-objects",
      "/building-sections",
      "/floors",
      "/unloading-points",
      "/construction-takts",
    ].some((p) => pathname === p || pathname.startsWith(p + "/"))
  )
    return "construction";
  if (pathname.startsWith("/storage")) return "storage";
  if (
    ["/carriers", "/vehicle-types", "/vehicles", "/transport-routes"].some(
      (p) => pathname === p || pathname.startsWith(p + "/"),
    )
  )
    return "logistics";
  return null;
}

function selectedKeyForPath(pathname: string): string[] {
  if (pathname.startsWith("/admin")) return [pathname];
  if (
    ["/product-types", "/plants", "/production-lines", "/line-capabilities"].some(
      (p) => pathname === p || pathname.startsWith(p + "/"),
    )
  ) {
    const match = ["/product-types", "/plants", "/production-lines", "/line-capabilities"].find(
      (p) => pathname === p || pathname.startsWith(p + "/"),
    );
    return match ? [match] : ["/"];
  }
  if (
    [
      "/construction-objects/new",
      "/construction-objects",
      "/building-sections",
      "/floors",
      "/unloading-points",
      "/construction-takts",
    ].some((p) => pathname === p || pathname.startsWith(p + "/"))
  ) {
    const match = [
      "/construction-objects/new",
      "/construction-objects",
      "/building-sections",
      "/floors",
      "/unloading-points",
      "/construction-takts",
    ].find((p) => pathname === p || pathname.startsWith(p + "/"));
    return match ? [match] : ["/construction-objects"];
  }
  if (pathname.startsWith("/storage")) return ["/storage-areas"];
  if (
    ["/carriers", "/vehicle-types", "/vehicles", "/transport-routes"].some(
      (p) => pathname === p || pathname.startsWith(p + "/"),
    )
  ) {
    const match = ["/carriers", "/vehicle-types", "/vehicles", "/transport-routes"].find(
      (p) => pathname === p || pathname.startsWith(p + "/"),
    );
    return match ? [match] : ["/carriers"];
  }
  return ["/products"];
}

export default function AppLayout() {
  const location = useLocation();
  const user = useSelector((s: RootState) => s.auth.user);
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const admin = isAdmin(user?.roles);

  const [openKeys, setOpenKeys] = useState<string[]>(() => {
    const s = sectionForPath(location.pathname);
    return s ? [s] : ["production"];
  });

  useEffect(() => {
    const s = sectionForPath(location.pathname);
    if (s)
      setOpenKeys((prev) => (prev.includes(s) ? prev : [...prev, s]));
  }, [location.pathname]);

  const items: MenuProps["items"] = useMemo(() => {
    const base: MenuProps["items"] = [
      {
        key: "/products",
        icon: <AppstoreOutlined />,
        label: <Link to="/products">Изделия</Link>,
      },
      {
        key: "production",
        icon: <BankOutlined />,
        label: "Производство",
        children: [
          {
            key: "/product-types",
            label: <Link to="/product-types">Типы изделий</Link>,
          },
          {
            key: "/plants",
            label: <Link to="/plants">Заводы</Link>,
          },
          {
            key: "/production-lines",
            label: <Link to="/production-lines">Линии</Link>,
          },
          {
            key: "/line-capabilities",
            label: <Link to="/line-capabilities">Возможности линий</Link>,
          },
        ],
      },
      {
        key: "construction",
        icon: <BuildOutlined />,
        label: "Строительство",
        children: [
          {
            key: "/construction-objects",
            label: <Link to="/construction-objects">Объекты</Link>,
          },
          {
            key: "/construction-objects/new",
            label: <Link to="/construction-objects/new">Добавить объект</Link>,
          },
          {
            key: "/building-sections",
            label: <Link to="/building-sections">Секции</Link>,
          },
          {
            key: "/floors",
            label: <Link to="/floors">Этажи</Link>,
          },
          {
            key: "/unloading-points",
            label: <Link to="/unloading-points">Точки разгрузки</Link>,
          },
          {
            key: "/construction-takts",
            label: <Link to="/construction-takts">Такты</Link>,
          },
        ],
      },
      {
        key: "storage",
        icon: <DatabaseOutlined />,
        label: "Склад",
        children: [
          {
            key: "/storage-areas",
            label: <Link to="/storage-areas">Склады</Link>,
          },
        ],
      },
      {
        key: "logistics",
        icon: <CarOutlined />,
        label: "Логистика",
        children: [
          {
            key: "/carriers",
            label: <Link to="/carriers">Перевозчики</Link>,
          },
          {
            key: "/vehicle-types",
            label: <Link to="/vehicle-types">Типы ТС</Link>,
          },
          {
            key: "/vehicles",
            label: <Link to="/vehicles">Транспорт</Link>,
          },
          {
            key: "/transport-routes",
            label: <Link to="/transport-routes">Маршруты</Link>,
          },
        ],
      },
    ];

    if (admin) {
      base!.push({
        key: "admin",
        icon: <SettingOutlined />,
        label: "Администрирование",
        children: [
          {
            key: "/admin",
            label: <Link to="/admin">Обзор</Link>,
          },
          {
            key: "/admin/roles",
            icon: <TeamOutlined />,
            label: <Link to="/admin/roles">Роли</Link>,
          },
          {
            key: "/admin/audit",
            icon: <AuditOutlined />,
            label: <Link to="/admin/audit">Журнал аудита</Link>,
          },
        ],
      });
    }

    return base;
  }, [admin]);

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
          selectedKeys={selectedKeyForPath(location.pathname)}
          openKeys={openKeys}
          onOpenChange={(keys) => setOpenKeys(keys as string[])}
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

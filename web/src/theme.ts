import type { ThemeConfig } from "antd";

/**
 * Фирменные цвета ГК «СУ-555».
 */
export const brand = {
  /** Основной сине-голубой */
  deepBlue: "#004D69",
  /** Акцентный небесно-голубой */
  skyBlue: "#2683C0",
  white: "#FFFFFF",
} as const;

/** Производные оттенки для интерфейса — затемнения и осветления фирменной пары. */
export const shades = {
  deepBlueDark: "#003A50",
  deepBlueSoft: "#0A6389",
  skyBlueDark: "#1F6C9F",
  skyBlueSoft: "#E8F2F9",
  skyBlueBorder: "#BBD8EC",
} as const;

/** Цвета статусов / вердиктов. */
export const verdictColors = {
  conforms: "#237804",
  conformsBg: "#F6FFED",
  notConforms: "#CF1322",
  notConformsBg: "#FFF1F0",
  pending: "#8C8C8C",
  pendingBg: "#FAFAFA",
  warning: "#D46B08",
  warningBg: "#FFF7E6",
} as const;

export const fontFamily =
  "'Roboto', -apple-system, BlinkMacSystemFont, 'Segoe UI', Arial, sans-serif";

export const antdTheme: ThemeConfig = {
  token: {
    colorPrimary: brand.skyBlue,
    colorInfo: brand.skyBlue,
    colorSuccess: verdictColors.conforms,
    colorError: verdictColors.notConforms,
    colorWarning: verdictColors.warning,
    colorLink: brand.skyBlue,
    colorTextHeading: brand.deepBlue,
    fontFamily,
    borderRadius: 4,
    controlHeight: 34,
  },
  components: {
    Layout: {
      headerBg: brand.deepBlue,
      headerHeight: 56,
      headerPadding: "0 20px",
      bodyBg: "#F4F6F8",
      siderBg: brand.deepBlue,
    },
    Menu: {
      darkItemBg: brand.deepBlue,
      darkSubMenuItemBg: shades.deepBlueDark,
      darkItemSelectedBg: brand.skyBlue,
      darkItemHoverBg: shades.deepBlueSoft,
      darkItemColor: "rgba(255,255,255,0.85)",
      darkItemSelectedColor: "#FFFFFF",
      iconSize: 16,
    },
    Table: {
      headerBg: shades.skyBlueSoft,
      headerColor: brand.deepBlue,
      rowHoverBg: "#F0F7FC",
    },
    Card: {
      headerBg: "#FFFFFF",
    },
    Descriptions: {
      labelBg: shades.skyBlueSoft,
    },
  },
};

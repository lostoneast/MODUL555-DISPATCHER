import { brand } from "../theme";

/**
 * Фирменный знак: две панели-рамки в перспективе.
 * Верхняя составная часть знака допускается к использованию самостоятельно.
 */
export function LogoMark({
  size = 28,
  light = false,
}: {
  size?: number;
  light?: boolean;
}) {
  const back = light ? "#FFFFFF" : brand.deepBlue;
  const front = light ? "#8FC3E4" : brand.skyBlue;
  const frame =
    "M 0 25 L 100 0 L 100 174.44 L 0 125 Z M 24 43 L 76 30 L 76 138.57 L 24 112.87 Z";
  return (
    <svg
      viewBox="0 0 139 224"
      height={size}
      width={(size * 139) / 224}
      role="img"
      aria-label="Модуль-555"
      style={{ display: "block", flexShrink: 0 }}
    >
      <path fill={back} fillRule="evenodd" transform="translate(38.15 0)" d={frame} />
      <path fill={front} fillRule="evenodd" transform="translate(0 48.89)" d={frame} />
    </svg>
  );
}

/**
 * Фирменный блок: знак + название бренда.
 * Пропорции знака фиксированы; искажения не допускаются.
 */
export function Logo({
  light = false,
  markSize = 34,
}: {
  light?: boolean;
  markSize?: number;
}) {
  const nameColor = light ? "#FFFFFF" : brand.deepBlue;
  const descriptorColor = light ? "#8FC3E4" : brand.skyBlue;
  return (
    <div style={{ display: "flex", alignItems: "center", gap: markSize * 0.28 }}>
      <LogoMark size={markSize} light={light} />
      <div style={{ lineHeight: 1.05 }}>
        <div
          style={{
            fontWeight: 700,
            fontSize: markSize * 0.56,
            letterSpacing: "0.01em",
            color: nameColor,
            whiteSpace: "nowrap",
          }}
        >
          МОДУЛЬ-555
        </div>
        <div
          style={{
            fontWeight: 400,
            fontSize: markSize * 0.235,
            letterSpacing: "0.02em",
            color: descriptorColor,
            whiteSpace: "nowrap",
          }}
        >
          ГРУППА КОМПАНИЙ «СУ-555»
        </div>
      </div>
    </div>
  );
}

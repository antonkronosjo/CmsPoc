import { useThemeMode } from "../theme/ThemeModeProvider";

// "cms" inherits the surrounding text color (via currentColor) so it stays readable
// against the AppBar, which is a plain white/dark-gray surface rather than a fixed
// brand color and flips per theme mode. ":es" gets its own per-mode accent so it still
// reads as a distinct color pop in both mode, since fill="currentColor" only gives it one shared color.
const ACCENT = { light: "#00838F", dark: "#5EEAD4" } as const;

export default function CmsEsLogo({ height = 22 }: { height?: number }) {
  const { mode } = useThemeMode();

  return (
    <svg height={height} viewBox="0 0 108 32" xmlns="http://www.w3.org/2000/svg" role="img" aria-labelledby="cmsEsLogoTitle">
      <title id="cmsEsLogoTitle">cms:es</title>
      <text x="0" y="23" fontFamily="Inter, Roboto, Helvetica, Arial, sans-serif" fontSize="22" fontWeight="800" letterSpacing="-0.5" fill="currentColor">
        cms
      </text>
      <text x="46" y="23" fontFamily="Inter, Roboto, Helvetica, Arial, sans-serif" fontSize="22" fontWeight="800" letterSpacing="-0.5" fill={ACCENT[mode]}>
        :es
      </text>
    </svg>
  );
}

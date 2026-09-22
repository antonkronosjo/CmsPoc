/// Black or white, whichever reads better on the given #RGB / #RRGGBB background.
export function contrastText(hex: string): string {
  const h = hex.length === 4 ? hex.slice(1).replace(/./g, "$&$&") : hex.slice(1);
  const [r, g, b] = [0, 2, 4].map((i) => parseInt(h.slice(i, i + 2), 16));
  return (r * 299 + g * 587 + b * 114) / 1000 > 150 ? "#000000" : "#FFFFFF";
}

/// Slightly rounded corners plus a flat fill of the given base color, with whichever of
/// black/white reads better against it, for chips (content type, language) that are
/// configured with a single color.
export function chipColorSx(baseHex: string): { borderRadius: string; backgroundColor: string; color: string } {
  return {
    borderRadius: "3px",
    backgroundColor: baseHex,
    color: contrastText(baseHex),
  };
}

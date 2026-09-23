import type { TypographyProps } from "@mui/material";

/// Maps markdown heading levels onto theme typography variants. The public pages use
/// h3 for the page title, so h2 inside body text has to be visually smaller than that.
/// Shared by MarkdownContent and MarkdownEditor so what editors see while typing matches
/// the rendered page.
export const markdownHeadingVariants: Record<"h2" | "h3" | "h4" | "h5" | "h6", TypographyProps["variant"]> = {
  h2: "h4",
  h3: "h5",
  h4: "h6",
  h5: "subtitle1",
  h6: "subtitle2",
};

import type { ReactNode } from "react";
import { Box, Link, Typography } from "@mui/material";
import Markdown from "react-markdown";
import { markdownHeadingVariants } from "../lib/markdownHeadings";

/// The markdown subset MarkdownEditor can produce. Anything else (hand-written markdown,
/// older content) is unwrapped to its text instead of rendering unstyled elements.
const ALLOWED_ELEMENTS = ["p", "br", "strong", "em", "a", "ul", "li", "h2", "h3", "h4", "h5", "h6"];

function heading(tag: keyof typeof markdownHeadingVariants) {
  return ({ children }: { children?: ReactNode }) => (
    <Typography variant={markdownHeadingVariants[tag]} component={tag} sx={{ fontWeight: 700, mb: 1, "* + &": { mt: 3 } }}>
      {children}
    </Typography>
  );
}

/// Renders a markdown property value (InputType.Markdown) with the app's typography.
/// Raw HTML is never rendered and unsafe link protocols are stripped by react-markdown.
export default function MarkdownContent({ children }: { children?: string }) {
  return (
    <Box sx={{ "& > :last-child": { mb: 0 } }}>
      <Markdown
        allowedElements={ALLOWED_ELEMENTS}
        unwrapDisallowed
        components={{
          h2: heading("h2"),
          h3: heading("h3"),
          h4: heading("h4"),
          h5: heading("h5"),
          h6: heading("h6"),
          p: ({ children }) => <Typography sx={{ mb: 2 }}>{children}</Typography>,
          a: ({ href, children }) => (
            <Link href={href} target="_blank" rel="noopener noreferrer">
              {children}
            </Link>
          ),
          ul: ({ children }) => (
            <Box component="ul" sx={{ pl: 3, mt: 0, mb: 2, typography: "body1" }}>
              {children}
            </Box>
          ),
          li: ({ children }) => <Box component="li" sx={{ mb: 0.5, "& p": { mb: 0 } }}>{children}</Box>,
        }}
      >
        {children ?? ""}
      </Markdown>
    </Box>
  );
}

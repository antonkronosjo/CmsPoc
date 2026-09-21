import { Link as RouterLink } from "react-router-dom";
import { CardActionArea, Grid } from "@mui/material";
import type { ContentSummaryDto } from "../../api/client";
import { contentPath } from "../../lib/contentPaths";
import DefaultContentTemplate from "./DefaultContent/DefaultContentTemplate";
import EventContentTemplate from "./EventContent/EventContentTemplate";
import NewsContentTemplate from "./NewsContent/NewsContentTemplate";
import SpecialNewsContentTemplate from "./SpecialNewsContent/SpecialNewsContentTemplate";
import type { ContentTemplate } from "./types";

/// One template per content type, keyed by ContentTypeKey. Add a folder and an
/// entry here when a new content type is introduced.
const templates: Record<string, ContentTemplate> = {
  EventContent: EventContentTemplate,
  NewsContent: NewsContentTemplate,
  SpecialNewsContent: SpecialNewsContentTemplate,
};

interface ContentAreaProps {
  content: ContentSummaryDto[];
}

/// Renders a list of content items, resolving each item's template from its
/// content type. Types without a template fall back to the default card.
export default function ContentArea({ content }: ContentAreaProps) {
  return (
    <Grid container spacing={2}>
      {content.map((item) => {
        const Template = templates[item.contentTypeKey] ?? DefaultContentTemplate;
        const path = contentPath(item.contentTypeKey, item.id);
        return (
          <Grid key={item.id} size={{ xs: 12, sm: 6, md: 4 }}>
            {path ? (
              <CardActionArea component={RouterLink} to={path} sx={{ height: "100%", borderRadius: 1 }}>
                <Template content={item} />
              </CardActionArea>
            ) : (
              <Template content={item} />
            )}
          </Grid>
        );
      })}
    </Grid>
  );
}

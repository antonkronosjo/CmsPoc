import { Card, CardContent, Chip, Typography } from "@mui/material";
import { getProperty, type ContentTemplateProps } from "../types";

export default function NewsContentTemplate({ content }: ContentTemplateProps) {
  const heading = getProperty(content, "Heading") || content.name;
  const body = getProperty(content, "Body");

  return (
    <Card sx={{ height: "100%" }}>
      <CardContent>
        <Chip size="small" label="News" sx={{ mb: 1 }} />
        <Typography variant="h6" gutterBottom>
          {heading}
        </Typography>
        <Typography variant="body2" color="text.secondary">
          {body}
        </Typography>
      </CardContent>
    </Card>
  );
}

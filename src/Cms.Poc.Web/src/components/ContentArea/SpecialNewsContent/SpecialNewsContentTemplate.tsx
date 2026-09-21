import { Card, CardContent, Chip, Typography } from "@mui/material";
import { getProperty, type ContentTemplateProps } from "../types";

export default function SpecialNewsContentTemplate({ content }: ContentTemplateProps) {
  const heading = getProperty(content, "Heading") || content.name;
  const body = getProperty(content, "Body");
  const specialBody = getProperty(content, "SpecialBody");

  return (
    <Card sx={{ height: "100%", borderColor: "warning.main", borderWidth: 2, borderStyle: "solid" }}>
      <CardContent>
        <Chip size="small" color="warning" label="Special news" sx={{ mb: 1 }} />
        <Typography variant="h6" gutterBottom>
          {heading}
        </Typography>
        <Typography variant="body2" color="text.secondary" gutterBottom>
          {body}
        </Typography>
        <Typography variant="body2">{specialBody}</Typography>
      </CardContent>
    </Card>
  );
}

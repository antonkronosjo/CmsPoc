import { Stack, Typography } from "@mui/material";
import { getProperty } from "../../components/ContentArea/types";
import ContentTypeChip from "../../components/ContentTypeChip";
import ContentPageShell from "./ContentPageShell";

export default function SpecialNewsPage() {
  return (
    <ContentPageShell contentTypeKey="SpecialNewsContent">
      {(content) => (
        <Stack spacing={2}>
          <ContentTypeChip contentTypeKey={content.contentTypeKey} label="Special news" sx={{ alignSelf: "flex-start" }} />
          <Typography variant="h3" component="h1">
            {getProperty(content, "Heading") || content.name}
          </Typography>
          <Typography sx={{ whiteSpace: "pre-line" }}>{getProperty(content, "Body")}</Typography>
          <Typography sx={{ whiteSpace: "pre-line", fontWeight: 500 }}>{getProperty(content, "SpecialBody")}</Typography>
        </Stack>
      )}
    </ContentPageShell>
  );
}

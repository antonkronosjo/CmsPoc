import { Chip, Stack, Typography } from "@mui/material";
import { getProperty } from "../../components/ContentArea/types";
import ContentPageShell from "./ContentPageShell";

export default function NewsPage() {
  return (
    <ContentPageShell contentTypeKey="NewsContent">
      {(content) => (
        <Stack spacing={2}>
          <Chip size="small" label="News" sx={{ alignSelf: "flex-start" }} />
          <Typography variant="h3" component="h1">
            {getProperty(content, "Heading") || content.name}
          </Typography>
          <Typography sx={{ whiteSpace: "pre-line" }}>{getProperty(content, "Body")}</Typography>
        </Stack>
      )}
    </ContentPageShell>
  );
}

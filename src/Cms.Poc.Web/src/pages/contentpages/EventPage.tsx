import { Chip, Stack, Typography } from "@mui/material";
import { useTranslation } from "react-i18next";
import { getProperty } from "../../components/ContentArea/types";
import EventDateInfo, { EventStatusChip } from "../../components/EventDateInfo";
import MarkdownContent from "../../components/MarkdownContent";
import ContentPageShell from "./ContentPageShell";

export default function EventPage() {
  const { t } = useTranslation();
  return (
    <ContentPageShell contentTypeKey="EventContent">
      {(content) => {
        const startDate = getProperty(content, "StartDate");
        const endDate = getProperty(content, "EndDate") || undefined;
        return (
          <Stack spacing={2}>
            <Stack direction="row" sx={{ justifyContent: "space-between", alignItems: "center" }}>
              <Chip size="small" label={t("contentType.event")} />
              <EventStatusChip startDate={startDate} endDate={endDate} />
            </Stack>
            <Typography variant="h3" component="h1">
              {getProperty(content, "Title") || content.name}
            </Typography>
            <Typography variant="h6" component="p" sx={{ fontWeight: 500, whiteSpace: "pre-line" }}>
              {getProperty(content, "Intro")}
            </Typography>
            {startDate && <EventDateInfo startDate={startDate} endDate={endDate} />}
            <MarkdownContent>{getProperty(content, "Description")}</MarkdownContent>
          </Stack>
        );
      }}
    </ContentPageShell>
  );
}

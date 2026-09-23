import { Stack, Typography } from "@mui/material";
import dayjs from "dayjs";
import { useTranslation } from "react-i18next";
import { getProperty } from "../../components/ContentArea/types";
import ContentTypeChip from "../../components/ContentTypeChip";
import ContentPageShell from "./ContentPageShell";

export default function EventPage() {
  const { t } = useTranslation();
  return (
    <ContentPageShell contentTypeKey="EventContent">
      {(content) => {
        const startDate = getProperty(content, "StartDate");
        return (
          <Stack spacing={2}>
            <ContentTypeChip contentTypeKey={content.contentTypeKey} label={t("contentType.event")} sx={{ alignSelf: "flex-start" }} />
            <Typography variant="h3" component="h1">
              {getProperty(content, "Title") || content.name}
            </Typography>
            {startDate && (
              <Typography variant="h6" color="text.secondary">
                {dayjs(startDate).format("dddd D MMMM YYYY, HH:mm")}
              </Typography>
            )}
            <Typography sx={{ whiteSpace: "pre-line" }}>{getProperty(content, "Description")}</Typography>
          </Stack>
        );
      }}
    </ContentPageShell>
  );
}

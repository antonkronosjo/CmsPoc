import { Box, Container, Grid, Link, Stack, Typography } from "@mui/material";
import { useTranslation } from "react-i18next";

/// Static placeholder footer for the public site. Content is mock/dummy - not
/// backed by real links or data.
export default function Footer() {
  const { t } = useTranslation();
  const year = new Date().getFullYear();

  return (
    <Box
      component="footer"
      sx={{ mt: "auto", borderTop: 1, borderColor: "divider", bgcolor: "background.paper" }}
    >
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Grid container spacing={4}>
          <Grid size={{ xs: 12, sm: 4 }}>
            <Typography variant="subtitle2" gutterBottom>
              {t("footer.about.title")}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              {t("footer.about.text")}
            </Typography>
          </Grid>
          <Grid size={{ xs: 12, sm: 4 }}>
            <Typography variant="subtitle2" gutterBottom>
              {t("footer.links.title")}
            </Typography>
            <Stack spacing={0.5}>
              <Link href="#" underline="hover" color="text.secondary" variant="body2">
                {t("footer.links.about")}
              </Link>
              <Link href="#" underline="hover" color="text.secondary" variant="body2">
                {t("footer.links.news")}
              </Link>
              <Link href="#" underline="hover" color="text.secondary" variant="body2">
                {t("footer.links.events")}
              </Link>
              <Link href="#" underline="hover" color="text.secondary" variant="body2">
                {t("footer.links.contact")}
              </Link>
            </Stack>
          </Grid>
          <Grid size={{ xs: 12, sm: 4 }}>
            <Typography variant="subtitle2" gutterBottom>
              {t("footer.contact.title")}
            </Typography>
            <Stack spacing={0.5}>
              <Typography variant="body2" color="text.secondary">
                {t("footer.contact.address")}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                {t("footer.contact.email")}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                {t("footer.contact.phone")}
              </Typography>
            </Stack>
          </Grid>
        </Grid>
        <Typography variant="body2" color="text.secondary" sx={{ mt: 4 }}>
          {t("footer.copyright", { year })}
        </Typography>
      </Container>
    </Box>
  );
}

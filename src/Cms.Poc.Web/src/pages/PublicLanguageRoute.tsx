import { Outlet, useParams } from "react-router-dom";
import { Alert, Container } from "@mui/material";
import { useLanguages } from "../hooks/useLanguages";

/// Guards the public site's /:language prefix: an unknown language is a 404
/// instead of being silently shown in the default language.
export default function PublicLanguageRoute() {
  const { language } = useParams();
  const { loaded, supportedLanguages } = useLanguages();

  if (!loaded) return null;
  if (!language || !supportedLanguages.includes(language)) {
    return (
      <Container maxWidth="md" sx={{ py: 4 }}>
        <Alert severity="warning">Page not found.</Alert>
      </Container>
    );
  }
  return <Outlet />;
}

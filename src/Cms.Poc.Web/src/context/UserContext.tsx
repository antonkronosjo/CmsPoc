import { createContext, useContext, type ReactNode } from "react";
import { useQuery } from "@tanstack/react-query";
import { api, CmsRole } from "../api/client";

interface UserContextValue {
  /// False until the backend has answered, and if it can't be reached - so anything gated on it stays hidden by default.
  isAuthenticated: boolean;
  displayName: string | null;
  roles: CmsRole[];
  /// Whether the user holds the role, counting Admin as also being an Editor (mirrors the backend).
  isInRole: (role: CmsRole) => boolean;
}

const UserContext = createContext<UserContextValue | undefined>(undefined);

export function useUser() {
  const context = useContext(UserContext);
  if (!context) throw new Error("useUser must be used within a UserProvider");
  return context;
}

export function UserProvider({ children }: { children: ReactNode }) {
  const { data } = useQuery({ queryKey: ["current-user"], queryFn: api.getCurrentUser, retry: false });

  const isAuthenticated = data?.isAuthenticated ?? false;
  const displayName = data?.displayName ?? null;
  const roles = data?.roles ?? [];
  const isInRole = (role: CmsRole) =>
    roles.includes(role) || (role === CmsRole.Editor && roles.includes(CmsRole.Admin));

  return (
    <UserContext.Provider value={{ isAuthenticated, displayName, roles, isInRole }}>{children}</UserContext.Provider>
  );
}

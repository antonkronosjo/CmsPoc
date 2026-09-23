import { useCallback, useState, type ReactNode } from "react";
import { Button, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle, type ButtonProps } from "@mui/material";
import { useTranslation } from "react-i18next";

interface ConfirmOptions {
  title: string;
  message: ReactNode;
  confirmText?: string;
  cancelText?: string;
  confirmColor?: ButtonProps["color"];
}

interface ConfirmState extends ConfirmOptions {
  resolve: (result: boolean) => void;
}

/// General-purpose "are you sure?" modal. Call `confirm(options)` from any
/// component and `await` the result instead of wiring up open/close state by
/// hand; render `<ConfirmDialog {...confirmDialogProps} />` once alongside it.
export function useConfirmDialog() {
  const [state, setState] = useState<ConfirmState | undefined>(undefined);

  const confirm = useCallback((options: ConfirmOptions) => {
    return new Promise<boolean>((resolve) => setState({ ...options, resolve }));
  }, []);

  function respond(result: boolean) {
    state?.resolve(result);
    setState(undefined);
  }

  const confirmDialogProps: ConfirmDialogProps = {
    open: !!state,
    title: state?.title ?? "",
    message: state?.message,
    confirmText: state?.confirmText,
    cancelText: state?.cancelText,
    confirmColor: state?.confirmColor,
    onConfirm: () => respond(true),
    onCancel: () => respond(false),
  };

  return { confirm, confirmDialogProps };
}

interface ConfirmDialogProps {
  open: boolean;
  title: string;
  message: ReactNode;
  confirmText?: string;
  cancelText?: string;
  confirmColor?: ButtonProps["color"];
  onConfirm: () => void;
  onCancel: () => void;
}

export default function ConfirmDialog({
  open,
  title,
  message,
  confirmText,
  cancelText,
  confirmColor = "primary",
  onConfirm,
  onCancel,
}: ConfirmDialogProps) {
  const { t } = useTranslation();
  return (
    <Dialog open={open} onClose={onCancel} maxWidth="xs" fullWidth>
      <DialogTitle>{title}</DialogTitle>
      <DialogContent>
        <DialogContentText>{message}</DialogContentText>
      </DialogContent>
      <DialogActions>
        <Button onClick={onCancel}>{cancelText ?? t("common.cancel")}</Button>
        <Button variant="contained" color={confirmColor} onClick={onConfirm} autoFocus>
          {confirmText ?? t("common.confirm")}
        </Button>
      </DialogActions>
    </Dialog>
  );
}

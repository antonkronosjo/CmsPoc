import { useCallback, useEffect, useRef } from "react";

export function useDebouncedCallback<TArgs extends unknown[]>(callback: (...args: TArgs) => void, delayMs: number) {
  const callbackRef = useRef(callback);
  callbackRef.current = callback;

  const timeoutRef = useRef<ReturnType<typeof setTimeout> | undefined>(undefined);

  useEffect(() => () => clearTimeout(timeoutRef.current), []);

  return useCallback(
    (...args: TArgs) => {
      clearTimeout(timeoutRef.current);
      timeoutRef.current = setTimeout(() => callbackRef.current(...args), delayMs);
    },
    [delayMs],
  );
}

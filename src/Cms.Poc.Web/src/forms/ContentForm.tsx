import { forwardRef, useImperativeHandle, useRef, useState } from "react";
import { Button, Stack, TextField, Tooltip, type TextFieldProps } from "@mui/material";
import { DatePicker, DateTimePicker } from "@mui/x-date-pickers";
import { api, InputType, type ContentPropertyValueDto, type ContentReference } from "../api/client";
import { useDebouncedCallback } from "../hooks/useDebouncedCallback";
import ContentPicker from "./ContentPicker";
import dayjs from "../lib/dayjs";

interface ContentFormProps {
  properties: Record<string, ContentPropertyValueDto>;
  contentTypeName: string;
  language: string;
  /// Set when editing an existing item: the language its shared (non-culture-specific)
  /// properties belong to. Shared properties are read-only in every other language.
  masterLanguage?: string;
  onChange: (key: string, value: unknown) => void;
  onSubmit: () => Promise<void> | void;
  submitText: string;
  disabled?: boolean;
  submitDisabled?: boolean;
  submitDisabledReason?: string;
}

/// One generic form for every content type. Adding a field to a content
/// type on the backend never touches this file - it just shows up here,
/// rendered by whichever InputType case matches its schema.
export default function ContentForm({
  properties,
  contentTypeName,
  language,
  masterLanguage,
  onChange,
  onSubmit,
  submitText,
  disabled,
  submitDisabled,
  submitDisabledReason,
}: ContentFormProps) {
  const fieldRefs = useRef<Record<string, FormElementHandle | null>>({});
  const [submitting, setSubmitting] = useState(false);

  const handleSubmit = async () => {
    const keys = Object.keys(properties);
    const results = await Promise.all(keys.map((key) => fieldRefs.current[key]?.validate() ?? Promise.resolve(true)));
    if (!results.every(Boolean)) return;

    setSubmitting(true);
    try {
      await onSubmit();
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Stack spacing={2} component="form" onSubmit={(e) => e.preventDefault()}>
      {Object.entries(properties).map(([key, valueDto]) => (
        <FormElementTemplate
          key={key}
          ref={(el) => {
            fieldRefs.current[key] = el;
          }}
          label={key}
          contentTypeName={contentTypeName}
          propertyName={key}
          valueDto={valueDto}
          disabled={disabled || (masterLanguage !== undefined && language !== masterLanguage && !valueDto.cultureSpecific)}
          note={sharedNote(valueDto, language, masterLanguage)}
          onChange={(value) => onChange(key, value)}
        />
      ))}
      <Tooltip title={!disabled && !submitting && submitDisabled ? submitDisabledReason ?? "" : ""}>
        <span style={{ alignSelf: "flex-end" }}>
          <Button variant="contained" fullWidth disabled={disabled || submitting || submitDisabled} onClick={handleSubmit}>
            {submitText}
          </Button>
        </span>
      </Tooltip>
    </Stack>
  );
}

/// Explains which fields are shared between languages, so it is clear why one is read-only.
function sharedNote(valueDto: ContentPropertyValueDto, language: string, masterLanguage: string | undefined): string | undefined {
  if (masterLanguage === undefined || valueDto.cultureSpecific) return undefined;
  return language === masterLanguage ? "Shared by all languages" : `Shared by all languages - edit in ${masterLanguage}`;
}

interface FormElementTemplateProps {
  label: string;
  contentTypeName: string;
  propertyName: string;
  valueDto: ContentPropertyValueDto;
  note?: string;
  onChange: (value: unknown) => void;
  disabled?: boolean;
}

interface FormElementHandle {
  validate: () => Promise<boolean>;
}

const FormElementTemplate = forwardRef<FormElementHandle, FormElementTemplateProps>(
  ({ label, propertyName, contentTypeName, valueDto, note, disabled, onChange }, ref) => {
    const [errors, setErrors] = useState<string[]>([]);
    const [touched, setTouched] = useState(false);

    const validate = async () => {
      setTouched(true);
      const result = await api.validateProperty(contentTypeName, propertyName, valueDto);
      setErrors(result);
      return result.length === 0;
    };

    const debouncedValidate = useDebouncedCallback(validate, 300);

    useImperativeHandle(ref, () => ({ validate }));

    const handleChange = (value: unknown) => {
      setTouched(true);
      onChange(value);
      if (touched) debouncedValidate();
    };

    const commonProps = {
      label,
      variant: "outlined",
      fullWidth: true,
      error: errors.length > 0,
      helperText: errors[0] ?? note ?? null,
      disabled,
      required: valueDto.required,
    } as TextFieldProps;

    switch (valueDto.inputType) {
      case InputType.Text:
        return <TextField {...commonProps} value={(valueDto.value as string) ?? ""} onChange={(e) => handleChange(e.target.value)} />;

      case InputType.TextArea:
        return (
          <TextField
            {...commonProps}
            multiline
            rows={5}
            value={(valueDto.value as string) ?? ""}
            onChange={(e) => handleChange(e.target.value)}
          />
        );

      case InputType.Number:
        return (
          <TextField
            {...commonProps}
            type="number"
            value={valueDto.value === null || valueDto.value === undefined ? "" : (valueDto.value as number)}
            onChange={(e) => handleChange(e.target.value === "" ? null : Number(e.target.value))}
          />
        );

      case InputType.Date:
        return (
          <DatePicker
            label={label}
            value={valueDto.value ? dayjs(valueDto.value as string) : null}
            onChange={(v) => handleChange(v ? v.format("YYYY-MM-DD") : null)}
            disabled={disabled}
            slotProps={{
              textField: {
                variant: "outlined",
                fullWidth: true,
                required: valueDto.required,
                error: errors.length > 0,
                helperText: errors[0] ?? note ?? null,
              },
            }}
          />
        );

      case InputType.DateTime:
        return (
          <DateTimePicker
            label={label}
            ampm={false}
            format="YYYY-MM-DD HH:mm"
            value={valueDto.value ? dayjs.utc(valueDto.value as string).local() : null}
            onChange={(v) => handleChange(v ? v.utc().toISOString() : null)}
            disabled={disabled}
            slotProps={{
              textField: {
                variant: "outlined",
                fullWidth: true,
                required: valueDto.required,
                error: errors.length > 0,
                helperText: errors[0] ?? note ?? null,
              },
            }}
          />
        );

      case InputType.ContentReference:
        return (
          <ContentPicker
            label={label}
            helperText={note}
            disabled={disabled}
            value={valueDto.value as ContentReference | null}
            onChange={(v) => handleChange(v)}
          />
        );

      default:
        return <>No template defined for property "{propertyName}"</>;
    }
  },
);

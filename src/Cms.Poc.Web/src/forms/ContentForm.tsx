import { forwardRef, useImperativeHandle, useRef, useState } from "react";
import { Button, Stack, TextField, type TextFieldProps } from "@mui/material";
import { DatePicker, DateTimePicker } from "@mui/x-date-pickers";
import { api, InputType, type ContentPropertyValueDto } from "../api/client";
import { useDebouncedCallback } from "../hooks/useDebouncedCallback";
import ContentPicker from "./ContentPicker";
import dayjs from "../lib/dayjs";

interface ContentFormProps {
  properties: Record<string, ContentPropertyValueDto>;
  contentTypeName: string;
  language: string;
  onChange: (key: string, value: unknown) => void;
  onSubmit: () => Promise<void> | void;
  submitText: string;
  disabled?: boolean;
}

/// One generic form for every content type. Adding a field to a content
/// type on the backend never touches this file - it just shows up here,
/// rendered by whichever InputType case matches its schema.
export default function ContentForm({ properties, contentTypeName, language, onChange, onSubmit, submitText, disabled }: ContentFormProps) {
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
          language={language}
          disabled={disabled}
          onChange={(value) => onChange(key, value)}
        />
      ))}
      <Button variant="contained" sx={{ alignSelf: "flex-end" }} disabled={disabled || submitting} onClick={handleSubmit}>
        {submitText}
      </Button>
    </Stack>
  );
}

interface FormElementTemplateProps {
  label: string;
  contentTypeName: string;
  propertyName: string;
  valueDto: ContentPropertyValueDto;
  language: string;
  onChange: (value: unknown) => void;
  disabled?: boolean;
}

interface FormElementHandle {
  validate: () => Promise<boolean>;
}

const FormElementTemplate = forwardRef<FormElementHandle, FormElementTemplateProps>(
  ({ label, propertyName, contentTypeName, valueDto, language, disabled, onChange }, ref) => {
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
      variant: "filled",
      fullWidth: true,
      error: errors.length > 0,
      helperText: errors[0] ?? null,
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
                variant: "filled",
                fullWidth: true,
                required: valueDto.required,
                error: errors.length > 0,
                helperText: errors[0] ?? null,
              },
            }}
          />
        );

      case InputType.DateTime:
        return (
          <DateTimePicker
            label={label}
            ampm={false}
            value={valueDto.value ? dayjs.utc(valueDto.value as string).local() : null}
            onChange={(v) => handleChange(v ? v.utc().toISOString() : null)}
            disabled={disabled}
            slotProps={{
              textField: {
                variant: "filled",
                fullWidth: true,
                required: valueDto.required,
                error: errors.length > 0,
                helperText: errors[0] ?? null,
              },
            }}
          />
        );

      case InputType.ContentReference:
        return (
          <ContentPicker
            label={label}
            language={language}
            disabled={disabled}
            value={valueDto.value as number | null}
            onChange={(v) => handleChange(v)}
          />
        );

      default:
        return <>No template defined for property "{propertyName}"</>;
    }
  },
);

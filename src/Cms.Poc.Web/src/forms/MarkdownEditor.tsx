import { forwardRef, useEffect, useLayoutEffect, useImperativeHandle, useRef, useState, type FocusEvent, type ReactNode } from "react";
import {
  Box,
  Button,
  Divider,
  IconButton,
  MenuItem,
  Popover,
  Select,
  Stack,
  TextField,
  Tooltip,
  type InputBaseComponentProps,
  type TextFieldProps,
} from "@mui/material";
import { FormatBold, FormatItalic, FormatListBulleted, InsertLink } from "@mui/icons-material";
import { useTranslation } from "react-i18next";
import { EditorContent, useEditor, useEditorState, type Editor } from "@tiptap/react";
import StarterKit from "@tiptap/starter-kit";
import { Markdown } from "@tiptap/markdown";
import { markdownHeadingVariants } from "../lib/markdownHeadings";

const HEADING_LEVELS = [2, 3, 4, 5, 6] as const;
type HeadingLevel = (typeof HEADING_LEVELS)[number];

const headingStyles = Object.fromEntries(
  Object.entries(markdownHeadingVariants).map(([tag, variant]) => [`& ${tag}`, { typography: variant, fontWeight: 700, mt: 0, mb: 1 }]),
);
// Space above a heading only when something precedes it (sibling selector rather than
// :first-child, which Emotion flags as SSR-unsafe).
const HEADING_TAGS = Object.keys(markdownHeadingVariants).join(", ");

/// "example.com" typed into the link box would otherwise become a relative link.
function withProtocol(href: string): string {
  return href === "" || /^(https?:|mailto:|tel:|\/|#)/i.test(href) ? href : `https://${href}`;
}

/// A TextField whose input is a WYSIWYG markdown editor (InputType.Markdown). Going through
/// TextField's inputComponent slot - rather than drawing an outline by hand - means the
/// label, notched outline, hover/focus/error/disabled states and helper text all come from
/// the same theme overrides as every other field in ContentForm.
export default function MarkdownTextField({ sx, slotProps, ...props }: TextFieldProps) {
  return (
    <TextField
      {...props}
      sx={[
        {
          "& .MuiInputBase-root": { p: 0, alignItems: "stretch" },
          "& .MuiInputBase-input": { height: "auto", p: 0 },
        },
        ...(Array.isArray(sx) ? sx : [sx]),
      ]}
      slotProps={{
        ...slotProps,
        // The toolbar always occupies the top of the field, so the label can never sit inside it.
        inputLabel: { shrink: true },
        input: { inputComponent: MarkdownEditorInput },
      }}
    />
  );
}

/// Adapts TipTap to MUI's inputComponent contract: take `value`, report changes as
/// `onChange({ target: { value } })`, forward focus/blur so the outline highlights, and
/// expose a ref with focus() so clicking the field's padding focuses the editor.
const MarkdownEditorInput = forwardRef<{ focus: () => void }, InputBaseComponentProps>(function MarkdownEditorInput(
  { value, onChange, onFocus, onBlur, disabled, className, id, "aria-describedby": ariaDescribedBy, "aria-invalid": ariaInvalid },
  ref,
) {
  const markdown = (value as string | undefined) ?? "";
  // The last value this editor emitted - lets the sync effect tell an external value change
  // (initial load, switching language, restoring a version) apart from the echo of our own typing.
  const lastEmitted = useRef(markdown);
  // TipTap captures onUpdate once, so read the latest onChange through a ref rather than a stale closure.
  const onChangeRef = useRef(onChange);
  useLayoutEffect(() => {
    onChangeRef.current = onChange;
  });

  const editor = useEditor({
    extensions: [
      StarterKit.configure({
        heading: { levels: [...HEADING_LEVELS] },
        // autolink off also makes the link mark non-inclusive, so typing right after a link
        // continues as plain text instead of silently extending the link.
        link: { openOnClick: false, autolink: false },
        blockquote: false,
        code: false,
        codeBlock: false,
        horizontalRule: false,
        orderedList: false,
        strike: false,
        underline: false,
      }),
      Markdown,
    ],
    content: markdown,
    contentType: "markdown",
    editable: !disabled,
    editorProps: {
      attributes: {
        ...(id ? { id } : {}),
        "aria-multiline": "true",
        ...(ariaDescribedBy ? { "aria-describedby": ariaDescribedBy } : {}),
        ...(ariaInvalid ? { "aria-invalid": "true" } : {}),
      },
    },
    onUpdate: ({ editor }) => {
      const next = editor.isEmpty ? "" : editor.getMarkdown().trim();
      lastEmitted.current = next;
      onChangeRef.current?.({ target: { value: next } } as never);
    },
  });

  useEffect(() => {
    if (!editor || markdown === lastEmitted.current) return;
    lastEmitted.current = markdown;
    editor.commands.setContent(markdown, { contentType: "markdown", emitUpdate: false });
  }, [editor, markdown]);

  useEffect(() => {
    editor?.setEditable(!disabled, false);
  }, [editor, disabled]);

  useImperativeHandle(ref, () => ({ focus: () => editor?.commands.focus() }), [editor]);

  // Focus moving between the toolbar, the text and the link popover and heading menu (portalled, but still
  // inside this React tree) should not flash the outline off and on, so only report blur
  // once focus has actually left all of them.
  const handleBlur = (e: FocusEvent<HTMLDivElement>) => {
    if (e.currentTarget.contains(e.relatedTarget as Node | null)) return;
    if (e.relatedTarget instanceof Element && e.relatedTarget.closest("[data-markdown-editor-popup]")) return;
    onBlur?.(e as never);
  };

  return (
    <Box className={className} onFocus={onFocus as never} onBlur={handleBlur} sx={{ display: "flex", flexDirection: "column" }}>
      {editor && <Toolbar editor={editor} disabled={!!disabled} />}
      <Divider />
      <Box
        component={EditorContent}
        editor={editor}
        sx={{
          "& .ProseMirror": {
            minHeight: 180,
            px: "14px",
            py: "12px",
            outline: "none",
            wordBreak: "break-word",
            typography: "body1",
            "& > :last-child": { mb: 0 },
            "& p": { m: 0, mb: 1.5 },
            "& ul": { pl: 3, mt: 0, mb: 1.5 },
            "& li > p": { mb: 0.5 },
            "& a": { color: "primary.main", textDecoration: "underline", cursor: "text" },
            ...headingStyles,
            [`& > * + :is(${HEADING_TAGS})`]: { mt: 2.5 },
          },
        }}
      />
    </Box>
  );
});

function Toolbar({ editor, disabled }: { editor: Editor; disabled: boolean }) {
  const { t } = useTranslation();
  const linkButton = useRef<HTMLButtonElement>(null);
  const [linkAnchor, setLinkAnchor] = useState<HTMLElement | null>(null);
  const [linkUrl, setLinkUrl] = useState("");

  const state = useEditorState({
    editor,
    selector: ({ editor }) => ({
      bold: editor.isActive("bold"),
      italic: editor.isActive("italic"),
      bulletList: editor.isActive("bulletList"),
      link: editor.isActive("link"),
      linkHref: (editor.getAttributes("link").href as string | undefined) ?? "",
      heading: (HEADING_LEVELS.find((level) => editor.isActive("heading", { level })) ?? 0) as HeadingLevel | 0,
    }),
  });

  const setBlockType = (level: HeadingLevel | 0) => {
    const chain = editor.chain().focus();
    (level === 0 ? chain.setParagraph() : chain.setHeading({ level })).run();
  };

  const openLink = () => {
    setLinkUrl(state.linkHref);
    setLinkAnchor(linkButton.current);
  };

  const closeLink = () => {
    setLinkAnchor(null);
    editor.commands.focus();
  };

  const applyLink = () => {
    const href = withProtocol(linkUrl.trim());
    if (!href) {
      editor.chain().focus().extendMarkRange("link").unsetLink().run();
    } else if (editor.state.selection.empty && !state.link) {
      // Nothing selected to turn into a link - insert the URL itself as the link text.
      editor
        .chain()
        .focus()
        .insertContent({ type: "text", text: href, marks: [{ type: "link", attrs: { href } }] })
        .run();
    } else {
      editor.chain().focus().extendMarkRange("link").setLink({ href }).run();
    }
    setLinkAnchor(null);
  };

  const removeLink = () => {
    editor.chain().focus().extendMarkRange("link").unsetLink().run();
    setLinkAnchor(null);
  };

  return (
    <Stack direction="row" spacing={0.25} sx={{ alignItems: "center", px: 1, py: 0.5, flexWrap: "wrap" }}>
      <Select
        size="small"
        variant="standard"
        disableUnderline
        disabled={disabled}
        value={state.heading}
        onChange={(e) => setBlockType(e.target.value as HeadingLevel | 0)}
        inputProps={{ "aria-label": t("markdownEditor.blockType") }}
        MenuProps={{ slotProps: { paper: { "data-markdown-editor-popup": true } as never } }}
        sx={{ minWidth: 120, mr: 0.5, "& .MuiSelect-select": { py: 0.5, pl: 0.5, typography: "body2", fontWeight: 500 } }}
      >
        <MenuItem value={0}>{t("markdownEditor.paragraph")}</MenuItem>
        {HEADING_LEVELS.map((level) => (
          <MenuItem key={level} value={level}>
            {t("markdownEditor.heading", { level })}
          </MenuItem>
        ))}
      </Select>
      <Divider orientation="vertical" flexItem sx={{ mx: 0.5 }} />
      <ToolbarButton
        label={t("markdownEditor.bold")}
        active={state.bold}
        disabled={disabled}
        onClick={() => editor.chain().focus().toggleBold().run()}
      >
        <FormatBold />
      </ToolbarButton>
      <ToolbarButton
        label={t("markdownEditor.italic")}
        active={state.italic}
        disabled={disabled}
        onClick={() => editor.chain().focus().toggleItalic().run()}
      >
        <FormatItalic />
      </ToolbarButton>
      <ToolbarButton
        label={t("markdownEditor.bulletList")}
        active={state.bulletList}
        disabled={disabled}
        onClick={() => editor.chain().focus().toggleBulletList().run()}
      >
        <FormatListBulleted />
      </ToolbarButton>
      <ToolbarButton ref={linkButton} label={t("markdownEditor.link")} active={state.link} disabled={disabled} onClick={openLink}>
        <InsertLink />
      </ToolbarButton>

      <Popover
        open={linkAnchor !== null}
        anchorEl={linkAnchor}
        onClose={closeLink}
        anchorOrigin={{ vertical: "bottom", horizontal: "left" }}
        slotProps={{ paper: { "data-markdown-editor-popup": true, sx: { p: 2, width: 360 } } as never }}
      >
        <Stack spacing={1.5}>
          <TextField
            autoFocus
            size="small"
            label={t("markdownEditor.linkUrl")}
            placeholder="https://"
            value={linkUrl}
            onChange={(e) => setLinkUrl(e.target.value)}
            onKeyDown={(e) => {
              if (e.key === "Enter") {
                e.preventDefault();
                applyLink();
              }
            }}
          />
          <Stack direction="row" spacing={1} sx={{ justifyContent: "flex-end" }}>
            {state.link && (
              <Button color="error" onClick={removeLink}>
                {t("markdownEditor.removeLink")}
              </Button>
            )}
            <Button onClick={closeLink}>{t("common.cancel")}</Button>
            <Button variant="contained" onClick={applyLink}>
              {t("markdownEditor.applyLink")}
            </Button>
          </Stack>
        </Stack>
      </Popover>
    </Stack>
  );
}

interface ToolbarButtonProps {
  label: string;
  active: boolean;
  disabled: boolean;
  onClick: () => void;
  children: ReactNode;
}

const ToolbarButton = forwardRef<HTMLButtonElement, ToolbarButtonProps>(function ToolbarButton(
  { label, active, disabled, onClick, children },
  ref,
) {
  return (
    <Tooltip title={label}>
      <span>
        <IconButton
          ref={ref}
          size="small"
          aria-label={label}
          aria-pressed={active}
          disabled={disabled}
          // Keep the text selection: without this the click moves focus to the button first.
          onMouseDown={(e) => e.preventDefault()}
          onClick={onClick}
          sx={{
            borderRadius: 0,
            color: active ? "primary.main" : "text.secondary",
            bgcolor: active ? "action.selected" : "transparent",
            "&:hover": { bgcolor: active ? "action.selected" : "action.hover" },
          }}
        >
          {children}
        </IconButton>
      </span>
    </Tooltip>
  );
});

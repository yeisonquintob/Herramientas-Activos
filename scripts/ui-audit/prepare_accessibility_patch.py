#!/usr/bin/env python3
"""Emite un parche apply_patch para identidad y asociación de campos NAVI."""

from __future__ import annotations

import argparse
import difflib
import re
from pathlib import Path

from audit_homologation import (
    BLAZOR_FIELD_TAG_RE,
    EXCLUDED,
    FIELD_TAG_RE,
    LABEL_BLOCK_RE,
    LOADING_STATUS_RE,
    PROTECTED_SET,
    ROOT,
    has_attr,
)


CONTROL_TAG_RE = re.compile(
    rf"(?:{FIELD_TAG_RE.pattern}|{BLAZOR_FIELD_TAG_RE.pattern})",
    re.I | re.S,
)
DISPLAY_DATA_LABEL_RE = re.compile(r"<label>(?P<body>.*?)</label>(?=\s*<strong\b)", re.I | re.S)


def slug(value: str) -> str:
    result = re.sub(r"[^a-z0-9]+", "-", value.lower()).strip("-")
    return result or "view"


def add_attribute(tag: str, attribute: str, value: str) -> str:
    stripped = tag.rstrip()
    suffix = tag[len(stripped):]
    if stripped.endswith("/>"):
        return stripped[:-2].rstrip() + f' {attribute}="{value}" />' + suffix
    return stripped[:-1].rstrip() + f' {attribute}="{value}">' + suffix


def apply_replacements(text: str, replacements: list[tuple[int, int, str]]) -> str:
    for start, end, value in sorted(replacements, reverse=True):
        text = text[:start] + value + text[end:]
    return text


def transform(path: Path, text: str) -> str:
    def loading_status(match: re.Match[str]) -> str:
        opening_end = match.group(0).find(">") + 1
        opening = match.group(0)[:opening_end]
        if not has_attr(opening, "role"):
            opening = add_attribute(opening, "role", "status")
        if not has_attr(opening, "aria-busy"):
            opening = add_attribute(opening, "aria-busy", "true")
        return opening + match.group(0)[opening_end:]

    text = LOADING_STATUS_RE.sub(loading_status, text)
    text = DISPLAY_DATA_LABEL_RE.sub(
        lambda match: f'<span class="navi-data-label">{match.group("body")}</span>',
        text,
    )
    prefix = f"navi-{slug(path.stem)}"
    replacements: list[tuple[int, int, str]] = []
    existing_controls = [int(value) for value in re.findall(rf'{re.escape(prefix)}-control-(\d+)', text)]
    paired = max(existing_controls, default=0)
    for match in LABEL_BLOCK_RE.finditer(text):
        attrs = match.group("attrs")
        body = match.group("body")
        if has_attr(attrs, "for") or CONTROL_TAG_RE.search(body):
            continue
        lookahead = text[match.end():match.end() + 2048]
        boundary = re.search(r"</(?:div|section|article)>|<label\b", lookahead, re.I)
        local = lookahead[:boundary.start()] if boundary else lookahead
        control = CONTROL_TAG_RE.search(local)
        control_offset = match.end()
        if not control:
            lookbehind_start = max(0, match.start() - 2048)
            lookbehind = text[lookbehind_start:match.start()]
            previous_boundary = list(re.finditer(r"</(?:div|section|article)>|<label\b", lookbehind, re.I))
            previous_local_start = previous_boundary[-1].end() if previous_boundary else 0
            previous_local = lookbehind[previous_local_start:]
            previous_controls = list(CONTROL_TAG_RE.finditer(previous_local))
            if previous_controls:
                control = previous_controls[-1]
                control_offset = lookbehind_start + previous_local_start
        if not control:
            continue
        paired += 1
        control_id = f"{prefix}-control-{paired:03d}"
        opening_end = text.find(">", match.start(), match.end())
        opening = text[match.start():opening_end + 1]
        replacements.append((match.start(), opening_end + 1, add_attribute(opening, "for", control_id)))
        control_start = control_offset + control.start()
        control_end = control_offset + control.end()
        control_tag = text[control_start:control_end]
        if not has_attr(control_tag, "id"):
            replacements.append((control_start, control_end, add_attribute(control_tag, "id", control_id)))

    updated = apply_replacements(text, replacements)
    replacements = []
    existing_fields = [int(value) for value in re.findall(rf'{re.escape(prefix)}-field-(\d+)', updated)]
    identity = max(existing_fields, default=0)
    for match in CONTROL_TAG_RE.finditer(updated):
        tag = match.group(0)
        if has_attr(tag, "id") or has_attr(tag, "name"):
            continue
        identity += 1
        name = f"{prefix}-field-{identity:03d}"
        replacements.append((match.start(), match.end(), add_attribute(tag, "name", name)))
    return apply_replacements(updated, replacements)


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--project", choices=("Admin", "Mobile"), required=True)
    args = parser.parse_args()
    root = ROOT / ("src/Navi.ToolsAssets.Admin" if args.project == "Admin" else "src/Navi.ToolsAssets.MobilePwa")
    sections: list[str] = ["*** Begin Patch"]
    changed = 0
    for path in sorted(root.rglob("*.razor")):
        relative = path.relative_to(ROOT).as_posix()
        if any(part in EXCLUDED for part in path.parts) or relative in PROTECTED_SET:
            continue
        before = path.read_text(encoding="utf-8-sig", errors="replace")
        after = transform(path, before)
        if after == before:
            continue
        diff = list(
            difflib.unified_diff(
                before.splitlines(keepends=True),
                after.splitlines(keepends=True),
                fromfile=relative,
                tofile=relative,
                n=3,
            )
        )
        sections.append(f"*** Update File: {path}")
        sections.extend("@@" if line.startswith("@@") else line.rstrip("\n") for line in diff[2:])
        changed += 1
    sections.append("*** End Patch")
    print("\n".join(sections))
    return 0 if changed else 2


if __name__ == "__main__":
    raise SystemExit(main())

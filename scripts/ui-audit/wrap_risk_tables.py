#!/usr/bin/env python3
"""Wrap Razor tables in the canonical horizontal-scroll container."""

from __future__ import annotations

import re
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
PROJECTS = (
    ROOT / "src/Navi.ToolsAssets.Admin",
    ROOT / "src/Navi.ToolsAssets.MobilePwa",
)
TABLE_RE = re.compile(r"<table\b[^>]*>.*?</table>", re.I | re.S)


def wrap_tables(text: str) -> tuple[str, int]:
    matches = list(TABLE_RE.finditer(text))
    changed = 0
    for match in reversed(matches):
        before = text[max(0, match.start() - 180):match.start()]
        if re.search(r'<div\s+class="[^"]*\bntx-table-scroll\b[^"]*">\s*$', before, re.I):
            continue
        line_start = text.rfind("\n", 0, match.start()) + 1
        indent = text[line_start:match.start()]
        if indent.strip():
            indent = re.match(r"\s*", indent).group(0)
        block = match.group(0)
        replacement = f'<div class="ntx-table-scroll">\n{indent}    {block}\n{indent}</div>'
        text = text[:match.start()] + replacement + text[match.end():]
        changed += 1
    return text, changed


def main() -> int:
    files_changed = 0
    tables_wrapped = 0
    for project in PROJECTS:
        for path in project.rglob("*.razor"):
            if "bin" in path.parts or "obj" in path.parts:
                continue
            original = path.read_text(encoding="utf-8-sig")
            updated, count = wrap_tables(original)
            if count:
                path.write_text(updated, encoding="utf-8")
                files_changed += 1
                tables_wrapped += count
    print(f"Wrapped {tables_wrapped} tables in {files_changed} Razor files")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

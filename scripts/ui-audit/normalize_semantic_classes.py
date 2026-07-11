#!/usr/bin/env python3
"""Replace chromatic class names with semantic visual-state names."""

from __future__ import annotations

import re
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
PROJECTS = (
    ROOT / "src/Navi.ToolsAssets.Admin",
    ROOT / "src/Navi.ToolsAssets.MobilePwa",
)
MAPPING = {
    "red": "tone-danger",
    "green": "tone-positive",
    "yellow": "tone-warning",
    "orange": "tone-warning",
    "blue": "tone-secondary",
    "cyan": "tone-secondary",
    "purple": "tone-secondary",
    "violet": "tone-secondary",
    "indigo": "tone-secondary",
    "gray": "tone-secondary",
    "grey": "tone-secondary",
}
CLASS_ATTRIBUTE_RE = re.compile(r'(\bclass\s*=\s*)(["\'])(.*?)(\2)', re.I | re.S)


def semantic_token(token: str) -> str:
    direct = MAPPING.get(token.lower())
    if direct:
        return direct
    parts = token.split("-")
    changed = False
    normalized: list[str] = []
    for part in parts:
        replacement = MAPPING.get(part.lower())
        if replacement:
            normalized.extend(replacement.split("-"))
            changed = True
        else:
            normalized.append(part)
    return "-".join(normalized) if changed else token


def normalize_razor(text: str) -> str:
    def replace_attribute(match: re.Match[str]) -> str:
        value = match.group(3)
        value = re.sub(r'(?<![@\w-])([A-Za-z_][\w-]*)', lambda token: semantic_token(token.group(1)), value)
        return f"{match.group(1)}{match.group(2)}{value}{match.group(4)}"

    return CLASS_ATTRIBUTE_RE.sub(replace_attribute, text)


def normalize_css(text: str) -> str:
    return re.sub(r'\.([A-Za-z_][\w-]*)', lambda match: "." + semantic_token(match.group(1)), text)


def remove_empty_rules(text: str) -> str:
    previous = None
    while previous != text:
        previous = text
        text = re.sub(r'(?m)(?:^|\n)[^{}]*\{\s*\}', "\n", text)
    text = re.sub(r'\n{3,}', "\n\n", text)
    return text.strip() + "\n"


def main() -> int:
    changed_razor = 0
    changed_css = 0
    for project in PROJECTS:
        for path in project.rglob("*.razor"):
            if "bin" in path.parts or "obj" in path.parts:
                continue
            original = path.read_text(encoding="utf-8-sig")
            updated = normalize_razor(original)
            if updated != original:
                path.write_text(updated, encoding="utf-8")
                changed_razor += 1
        for path in project.rglob("*.css"):
            if "bootstrap" in path.parts or "bin" in path.parts or "obj" in path.parts:
                continue
            original = path.read_text(encoding="utf-8-sig")
            updated = remove_empty_rules(normalize_css(original))
            if updated != original:
                path.write_text(updated, encoding="utf-8")
                changed_css += 1
    print(f"Semantic classes normalized in {changed_razor} Razor and {changed_css} CSS files")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

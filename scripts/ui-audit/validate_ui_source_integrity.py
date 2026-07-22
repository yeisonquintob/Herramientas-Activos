#!/usr/bin/env python3
"""Validaciones estructurales de CSS, JavaScript, hosts y rutas NAVI."""

from __future__ import annotations

import json
import re
from collections import Counter
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
PROJECTS = {
    "Admin": ROOT / "src/Navi.ToolsAssets.Admin",
    "Mobile": ROOT / "src/Navi.ToolsAssets.MobilePwa",
}
EXCLUDED = {"bin", "obj", "node_modules", "export_codigo", "backups", "backup"}
ROUTE_RE = re.compile(r'^\s*@page\s+"([^"]+)"', re.M)
VAR_DEFINITION_RE = re.compile(r"(?<![\w-])(--(?:navi|ntx)-[\w-]+)\s*:")
VAR_REFERENCE_RE = re.compile(r"var\(\s*(--(?:navi|ntx)-[\w-]+)")


def source_files(root: Path, suffix: str) -> list[Path]:
    return [
        path
        for path in sorted(root.rglob(f"*{suffix}"))
        if not any(part in EXCLUDED for part in path.parts)
        and not path.name.endswith(f".min{suffix}")
        and not path.name.endswith(".styles.css")
    ]


def mask_literals_and_comments(text: str, line_comments: bool) -> str:
    result: list[str] = []
    index = 0
    quote = ""
    escaped = False
    while index < len(text):
        char = text[index]
        next_char = text[index + 1] if index + 1 < len(text) else ""
        if quote:
            result.append("\n" if char == "\n" else " ")
            if escaped:
                escaped = False
            elif char == "\\":
                escaped = True
            elif char == quote:
                quote = ""
            index += 1
            continue
        if char in {'"', "'", "`"}:
            quote = char
            result.append(" ")
            index += 1
            continue
        if char == "/" and next_char == "*":
            result.extend((" ", " "))
            index += 2
            while index < len(text):
                if text[index:index + 2] == "*/":
                    result.extend((" ", " "))
                    index += 2
                    break
                result.append("\n" if text[index] == "\n" else " ")
                index += 1
            continue
        if line_comments and char == "/" and next_char == "/":
            while index < len(text) and text[index] != "\n":
                result.append(" ")
                index += 1
            continue
        result.append(char)
        index += 1
    return "".join(result)


def delimiter_errors(path: Path, pairs: dict[str, str], line_comments: bool) -> list[str]:
    text = path.read_text(encoding="utf-8-sig", errors="replace")
    masked = mask_literals_and_comments(text, line_comments)
    closing = {value: key for key, value in pairs.items()}
    stack: list[tuple[str, int]] = []
    errors: list[str] = []
    for offset, char in enumerate(masked):
        if char in pairs:
            stack.append((char, offset))
        elif char in closing:
            if not stack or stack[-1][0] != closing[char]:
                errors.append(f"{path.relative_to(ROOT)}:{masked.count(chr(10), 0, offset) + 1}: cierre {char} sin apertura")
                continue
            stack.pop()
    for char, offset in stack:
        errors.append(f"{path.relative_to(ROOT)}:{masked.count(chr(10), 0, offset) + 1}: apertura {char} sin cierre")
    return errors


def main() -> int:
    errors: list[str] = []
    summary: dict[str, object] = {}
    for project, root in PROJECTS.items():
        css_files = source_files(root, ".css")
        js_files = source_files(root, ".js")
        razor_files = source_files(root, ".razor")
        for path in css_files:
            errors.extend(delimiter_errors(path, {"{": "}", "(": ")", "[": "]"}, False))
        for path in js_files:
            errors.extend(delimiter_errors(path, {"{": "}", "(": ")", "[": "]"}, True))

        css_text = "\n".join(path.read_text(encoding="utf-8-sig", errors="replace") for path in css_files)
        definitions = set(VAR_DEFINITION_RE.findall(css_text))
        references = set(VAR_REFERENCE_RE.findall(css_text))
        unresolved = sorted(references - definitions)
        for variable in unresolved:
            errors.append(f"{project}: variable CSS sin definición: {variable}")

        routes: list[tuple[str, str]] = []
        for path in razor_files:
            text = path.read_text(encoding="utf-8-sig", errors="replace")
            routes.extend((route, path.relative_to(ROOT).as_posix()) for route in ROUTE_RE.findall(text))
        route_counts = Counter(route for route, _ in routes)
        duplicates = sorted(route for route, count in route_counts.items() if count > 1)
        for route in duplicates:
            owners = [path for candidate, path in routes if candidate == route]
            errors.append(f"{project}: ruta duplicada {route}: {', '.join(owners)}")

        summary[project] = {
            "razor_files": len(razor_files),
            "routes": len(routes),
            "css_files": len(css_files),
            "js_files": len(js_files),
            "canonical_css_definitions": len(definitions),
            "canonical_css_references": len(references),
            "unresolved_canonical_variables": len(unresolved),
            "duplicate_routes": len(duplicates),
        }

    hosts = {
        "Admin": PROJECTS["Admin"] / "Components/App.razor",
        "Mobile": PROJECTS["Mobile"] / "wwwroot/index.html",
    }
    expected_assets = {
        "Admin": ("navi-ui-preferences.js", "navi-ui-preferences.css"),
        "Mobile": ("navi-mobile-ui-preferences.js", "navi-mobile-ui-preferences.css"),
    }
    for project, path in hosts.items():
        text = path.read_text(encoding="utf-8-sig", errors="replace")
        for asset in expected_assets[project]:
            count = text.count(asset)
            if count != 1:
                errors.append(f"{project}: {asset} aparece {count} veces en el host")
        for invalid in ("navi-navi", "ntx-ntx", "= data-ntx-tone"):
            if invalid in text:
                errors.append(f"{project}: patrón inválido en host: {invalid}")

    print(json.dumps({"summary": summary, "errors": errors}, ensure_ascii=False, indent=2))
    return 1 if errors else 0


if __name__ == "__main__":
    raise SystemExit(main())

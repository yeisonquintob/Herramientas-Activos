#!/usr/bin/env python3
"""Auditor específico del prompt de homologación visual global NAVI."""

from __future__ import annotations

import argparse
import json
import re
from collections import Counter
from datetime import datetime
from pathlib import Path

from homologation_contract import EXCLUDED, PROTECTED, ROOT


PROJECTS = {
    "Admin": ROOT / "src/Navi.ToolsAssets.Admin",
    "Mobile": ROOT / "src/Navi.ToolsAssets.MobilePwa",
}
PROTECTED_SET = set(PROTECTED)
ROUTE_RE = re.compile(r'^\s*@page\s+"([^"]+)"', re.M)
TAG_BODY = r"(?:[^>\"']|\"[^\"]*\"|'[^']*')*"
FIELD_TAG_RE = re.compile(rf"<(input|select|textarea)\b{TAG_BODY}>", re.I | re.S)
BLAZOR_FIELD_TAG_RE = re.compile(
    rf"<Input(?:TextArea|Text|Select|Checkbox|File|Number|Date|Radio|RadioGroup)\b{TAG_BODY}>",
    re.I | re.S,
)
LABEL_BLOCK_RE = re.compile(rf"<label\b(?P<attrs>{TAG_BODY})>(?P<body>.*?)</label\s*>", re.I | re.S)
BUTTON_BLOCK_RE = re.compile(rf"<button\b(?P<attrs>{TAG_BODY})>(?P<body>.*?)</button\s*>", re.I | re.S)
LOADING_STATUS_RE = re.compile(
    rf"<(?P<tag>p|div|span)\b(?P<attrs>{TAG_BODY})>(?P<body>[^<]{{0,320}}\bCargando\b[^<]{{0,320}})</(?P=tag)\s*>",
    re.I | re.S,
)
ATTR_RE_TEMPLATE = r"\b{attribute}\s*=\s*(?:\"[^\"]*\"|'[^']*')"
GRADIENT_RE = re.compile(r"\b(?:linear|radial|conic)-gradient\(", re.I)
NOWRAP_RE = re.compile(r"white-space\s*:\s*nowrap", re.I)
ABSOLUTE_RE = re.compile(r"position\s*:\s*absolute", re.I)
OVERFLOW_AUTO_RE = re.compile(r"overflow-x\s*:\s*auto", re.I)
HTML_TAG_RE = re.compile(r"<[^>]+>", re.S)
ICON_ONLY_RE = re.compile(r"^(?:\s|<svg\b.*?</svg\s*>|<span\b[^>]*aria-hidden[^>]*>.*?</span\s*>)*$", re.I | re.S)


def relative(path: Path) -> str:
    return path.relative_to(ROOT).as_posix()


def line_number(text: str, offset: int) -> int:
    return text.count("\n", 0, offset) + 1


def has_attr(tag_or_attrs: str, attribute: str) -> bool:
    return bool(re.search(ATTR_RE_TEMPLATE.format(attribute=re.escape(attribute)), tag_or_attrs, re.I | re.S))


def is_source(path: Path) -> bool:
    return (
        path.is_file()
        and not any(part in EXCLUDED for part in path.parts)
        and not path.name.endswith((".min.css", ".min.js", ".styles.css"))
        and path.suffix.lower() in {".razor", ".css"}
    )


def add(findings: list[dict[str, object]], category: str, project: str, path: Path, text: str, offset: int, snippet: str, severity: str = "blocking") -> None:
    findings.append(
        {
            "category": category,
            "severity": severity,
            "project": project,
            "file": relative(path),
            "line": line_number(text, offset),
            "snippet": " ".join(snippet.split())[:300],
        }
    )


def scan_razor(project: str, path: Path, text: str, findings: list[dict[str, object]]) -> None:
    routes = ROUTE_RE.findall(text)
    protected = relative(path) in PROTECTED_SET
    if protected:
        return

    for match in FIELD_TAG_RE.finditer(text):
        tag = match.group(0)
        if not has_attr(tag, "id") and not has_attr(tag, "name"):
            add(findings, "field_without_id_or_name", project, path, text, match.start(), tag)
    for match in BLAZOR_FIELD_TAG_RE.finditer(text):
        tag = match.group(0)
        if not has_attr(tag, "id") and not has_attr(tag, "name"):
            add(findings, "field_without_id_or_name", project, path, text, match.start(), tag)

    for match in LABEL_BLOCK_RE.finditer(text):
        attrs = match.group("attrs")
        body = match.group("body")
        if has_attr(attrs, "for"):
            continue
        if FIELD_TAG_RE.search(body) or BLAZOR_FIELD_TAG_RE.search(body):
            continue
        lookahead = text[match.end():match.end() + 2048]
        boundary = re.search(r"</(?:div|section|article)>|<label\b", lookahead, re.I)
        local = lookahead[:boundary.start()] if boundary else lookahead
        lookbehind = text[max(0, match.start() - 2048):match.start()]
        previous_boundary = list(re.finditer(r"</(?:div|section|article)>|<label\b", lookbehind, re.I))
        previous_local = lookbehind[previous_boundary[-1].end():] if previous_boundary else lookbehind
        if (
            FIELD_TAG_RE.search(local)
            or BLAZOR_FIELD_TAG_RE.search(local)
            or FIELD_TAG_RE.search(previous_local)
            or BLAZOR_FIELD_TAG_RE.search(previous_local)
        ):
            add(findings, "label_without_for", project, path, text, match.start(), match.group(0))
        else:
            add(findings, "display_label_element", project, path, text, match.start(), match.group(0), "review")

    for match in BUTTON_BLOCK_RE.finditer(text):
        attrs = match.group("attrs")
        body = match.group("body")
        visible = HTML_TAG_RE.sub(" ", body)
        visible = re.sub(r"@[\w().? :\"'-]+", " ", visible).strip()
        has_visible_span = bool(re.search(r"<span\b(?![^>]*aria-hidden)[^>]*>", body, re.I))
        icon_only = not visible and not has_visible_span and ICON_ONLY_RE.match(body)
        if icon_only and not has_attr(attrs, "aria-label") and not has_attr(attrs, "title"):
            add(findings, "icon_button_without_label", project, path, text, match.start(), match.group(0))
        tone = re.search(r'data-ntx-tone\s*=\s*"expand"', attrs, re.I)
        toggle_semantics = has_attr(attrs, "aria-pressed") or (
            has_attr(attrs, "aria-expanded") and has_attr(attrs, "aria-controls")
        )
        if tone and not toggle_semantics:
            add(findings, "expand_without_aria", project, path, text, match.start(), match.group(0))

    for match in LOADING_STATUS_RE.finditer(text):
        if not has_attr(match.group("attrs"), "aria-busy"):
            add(findings, "loading_status_without_aria_busy", project, path, text, match.start(), match.group(0))

    if routes and project == "Admin" and not any(route in {"/login"} for route in routes):
        if "<NaviPageHero" not in text and not any(component in text for component in ("<SettingsCatalogCrud", "<SpcRequestWorkspace")):
            add(findings, "admin_header_missing", project, path, text, 0, ", ".join(routes))

    if routes and project == "Mobile":
        exempt = all(route in {"/login", "/counter", "/weather"} for route in routes)
        if not exempt and not any(marker in text for marker in ("<NaviMobileAppBanner", "<NaviMobilePageHero", "<NaviMobilePageHeaderState")):
            add(findings, "mobile_header_missing", project, path, text, 0, ", ".join(routes))


def scan_css(project: str, path: Path, text: str, findings: list[dict[str, object]]) -> None:
    if relative(path) in PROTECTED_SET:
        return
    is_view = path.name.endswith(".razor.css") or "/Pages/" in relative(path)
    for match in GRADIENT_RE.finditer(text):
        severity = "review" if not is_view else "blocking"
        add(findings, "gradient", project, path, text, match.start(), match.group(0), severity)
    if is_view:
        for match in NOWRAP_RE.finditer(text):
            add(findings, "nowrap_review", project, path, text, match.start(), match.group(0), "review")
        for match in ABSOLUTE_RE.finditer(text):
            add(findings, "absolute_position_review", project, path, text, match.start(), match.group(0), "review")
        for match in OVERFLOW_AUTO_RE.finditer(text):
            add(findings, "overflow_auto_review", project, path, text, match.start(), match.group(0), "review")


def write_report(mode: str, findings: list[dict[str, object]]) -> tuple[Path, Path]:
    suffix = "BASELINE" if mode == "baseline" else "FINAL"
    output = ROOT / "docs/parametrizacion"
    output.mkdir(parents=True, exist_ok=True)
    json_path = output / f"NAVI_HOMOLOGACION_AUDIT_{suffix}.json"
    md_path = output / f"NAVI_HOMOLOGACION_AUDIT_{suffix}.md"
    counts = Counter(item["category"] for item in findings)
    blocking = [item for item in findings if item["severity"] == "blocking"]
    generated = datetime.now().astimezone().isoformat(timespec="seconds")
    report = {
        "generated_at": generated,
        "mode": mode,
        "summary": {
            "total": len(findings),
            "blocking": len(blocking),
            "review": len(findings) - len(blocking),
            "passed": not blocking,
            "by_category": dict(sorted(counts.items())),
        },
        "findings": findings,
    }
    json_path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    lines = [
        f"# Auditoría de homologación NAVI — {suffix.lower()}",
        "",
        f"Generada: `{generated}`",
        "",
        f"- Bloqueantes: **{len(blocking)}**.",
        f"- Revisión manual: **{len(findings) - len(blocking)}**.",
        f"- Resultado: **{'APROBADO' if not blocking else 'NO APROBADO'}**.",
        "",
        "| Categoría | Total |",
        "|---|---:|",
    ]
    lines.extend(f"| `{category}` | {count} |" for category, count in sorted(counts.items()))
    lines.extend(["", "## Hallazgos", ""])
    for category in sorted(counts):
        lines.extend([f"### {category}", ""])
        items = [item for item in findings if item["category"] == category]
        for item in items[:80]:
            lines.append(f"- `{item['file']}:{item['line']}` — {item['snippet']}")
        if len(items) > 80:
            lines.append(f"- … {len(items) - 80} adicionales en JSON.")
        lines.append("")
    md_path.write_text("\n".join(lines).rstrip() + "\n", encoding="utf-8")
    return json_path, md_path


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--mode", choices=("baseline", "strict"), default="baseline")
    args = parser.parse_args()
    findings: list[dict[str, object]] = []
    for project, root in PROJECTS.items():
        for path in sorted(root.rglob("*")):
            if not is_source(path):
                continue
            text = path.read_text(encoding="utf-8-sig", errors="replace")
            if path.suffix.lower() == ".razor":
                scan_razor(project, path, text, findings)
            else:
                scan_css(project, path, text, findings)
    findings.sort(key=lambda item: (item["category"], item["file"], item["line"]))
    json_path, md_path = write_report(args.mode, findings)
    counts = Counter(item["category"] for item in findings)
    blocking = sum(item["severity"] == "blocking" for item in findings)
    print(json.dumps({"json": relative(json_path), "markdown": relative(md_path), "blocking": blocking, "review": len(findings) - blocking, "by_category": dict(sorted(counts.items()))}, ensure_ascii=False, indent=2))
    return 1 if args.mode == "strict" and blocking else 0


if __name__ == "__main__":
    raise SystemExit(main())

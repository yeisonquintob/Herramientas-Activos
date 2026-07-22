#!/usr/bin/env python3
"""Audit the NAVI appearance contract without changing source files.

Baseline mode is informational. Strict mode returns a non-zero exit code when
blocking visual debt remains. Generated files are intentionally kept under
docs/parametrizacion so the two UI applications can be reviewed independently.
"""

from __future__ import annotations

import argparse
import json
import re
from collections import Counter
from datetime import datetime
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
PROJECTS = {
    "Admin": ROOT / "src/Navi.ToolsAssets.Admin",
    "Mobile": ROOT / "src/Navi.ToolsAssets.MobilePwa",
}
CANONICAL_ACTION_TONES = {
    "affirmative",
    "refresh",
    "operation",
    "warning",
    "danger",
    "consult",
    "secondary",
    "expand",
    "ghost",
}
CANONICAL_KPI_TONES = {
    "neutral",
    "positive",
    "warning",
    "danger",
    "soft",
}
BLOCKING_CATEGORIES = {
    "direct_color",
    "important",
    "style_block",
    "inline_style",
    "button_without_tone",
    "noncanonical_tone",
    "manual_kpi",
    "mobile_header_missing",
    "overflow_x_hidden",
    "bootstrap_color",
    "chart_direct_color",
    "view_font_size",
    "duplicate_preference_asset",
    "action_tone_mismatch",
    "noncanonical_kpi_tone",
    "mobile_kpi_overflow_missing",
}
EXCLUDED_PARTS = {
    "bin",
    "obj",
    ".git",
    "export_codigo",
    "backups",
    "backup",
    "node_modules",
}
ALLOWED_COLOR_FILES = {
    "tokens.css",
    "navi-ui-preferences.css",
    "navi-mobile-ui-preferences.css",
    # Las muestras deben representar temas distintos sin cambiar el tema activo.
    "SettingsAppearance.razor.css",
    "MobileAppearanceSettingsPanel.razor.css",
}
HEADER_MARKERS = {
    "NaviMobileAppBanner",
    "NaviMobilePageHero",
    "NaviMobilePageHeaderState",
}

HEX_RE = re.compile(r"#[0-9a-fA-F]{3,8}\b")
RGB_RE = re.compile(r"\brgba?\([^)]*\)", re.I)
ROUTE_RE = re.compile(r'^\s*@page\s+"([^"]+)"', re.M)
STYLE_BLOCK_RE = re.compile(r"<style\b", re.I)
INLINE_STYLE_RE = re.compile(r"\bstyle\s*=", re.I)
JUSTIFIED_DYNAMIC_STYLE_RE = re.compile(
    r"style\s*=\s*\"(?:@Get[\w]+Style\s*\(|width\s*:\s*@|@\(\$\"width\s*:)",
    re.I,
)
IMPORTANT_RE = re.compile(r"!important\b", re.I)
OVERFLOW_HIDDEN_RE = re.compile(r"overflow-x\s*:\s*hidden", re.I)
FONT_SIZE_RE = re.compile(r"font-size\s*:\s*([^;}]+)", re.I)
FIXED_HEIGHT_RE = re.compile(r"(?<!min-)(?<!max-)height\s*:\s*(\d+(?:\.\d+)?px)\b", re.I)
BOOTSTRAP_COLOR_RE = re.compile(
    r"(?<!navi-)(?<!ntx-)\b"
    r"(?:(?:btn-(?:outline-)?|text-|text-bg-|bg-|border-|badge-)"
    r"(?:primary|success|info|warning|danger|dark))\b"
    r"|var\(--bs-(?:primary|success|info|warning|danger|dark)",
    re.I,
)
MANUAL_KPI_RE = re.compile(
    r'<article\b[^>]*\bclass\s*=\s*"[^"]*\b(?:kpi|metrics?|stats?|counter|counters)\b[^"]*"',
    re.I,
)
ACTION_TAG_RE = re.compile(
    r"<(button|NaviActionButton|NaviMobileActionButton)\b"
    r"(?:[^>\"']|\"[^\"]*\"|'[^']*')*>",
    re.I | re.S,
)
KPI_CARD_TAG_RE = re.compile(
    r"<(NaviKpiCard|NaviMobileKpiCard)\b"
    r"(?:[^>\"']|\"[^\"]*\"|'[^']*')*>",
    re.I | re.S,
)
ACTION_BLOCK_RE = re.compile(
    r"<(?P<tag>button|NaviActionButton|NaviMobileActionButton)\b"
    r"(?P<attrs>(?:[^>\"']|\"[^\"]*\"|'[^']*')*)>"
    r"(?P<body>.*?)</(?P=tag)\s*>",
    re.I | re.S,
)
DATA_TONE_RE = re.compile(r'\bdata-ntx-tone\s*=\s*"([^"]+)"', re.I)
COMPONENT_TONE_RE = re.compile(r'\bTone\s*=\s*"([^"]+)"', re.I)
ACTION_WORD_TONES = (
    (re.compile(r"\b(guardar|confirmar|validar|aceptar|aprobar|enviar|habilitar)\b", re.I), "affirmative"),
    (re.compile(r"\b(actualizar|recargar|reintentar)\b", re.I), "refresh"),
    (re.compile(r"\b(editar|asignar|gestionar|crear|solicitar|reportar|agregar|filtrar)\b", re.I), "operation"),
    (re.compile(r"\b(regresar|devolver|reabrir|retomar)\b", re.I), "warning"),
    (re.compile(r"\b(rechazar|denegar|eliminar|anular|deshabilitar|desactivar)\b", re.I), "danger"),
    (re.compile(r"\b(consultar|revisar|exportar|descargar)\b|ver\s+detalle", re.I), "consult"),
    (re.compile(r"\b(volver|atr[aá]s|limpiar|deshacer|cancelar)\b", re.I), "secondary"),
)


def relative(path: Path) -> str:
    return path.relative_to(ROOT).as_posix()


def is_source(path: Path) -> bool:
    if any(part in EXCLUDED_PARTS for part in path.parts):
        return False
    if path.name.endswith(".min.css") or path.name.endswith(".min.js"):
        return False
    if path.name.endswith(".styles.css"):
        return False
    return path.suffix.lower() in {".razor", ".css", ".js", ".html"}


def strip_comments(text: str) -> str:
    text = re.sub(r"/\*.*?\*/", "", text, flags=re.S)
    text = re.sub(r"@\*.*?\*@", "", text, flags=re.S)
    return re.sub(r"<!--.*?-->", "", text, flags=re.S)


def line_number(text: str, offset: int) -> int:
    return text.count("\n", 0, offset) + 1


def add_finding(
    findings: list[dict[str, object]],
    category: str,
    project: str,
    path: Path,
    text: str,
    offset: int,
    snippet: str,
    severity: str = "debt",
) -> None:
    findings.append(
        {
            "category": category,
            "severity": severity,
            "project": project,
            "file": relative(path),
            "line": line_number(text, offset),
            "snippet": " ".join(snippet.strip().split())[:280],
        }
    )


def scan_file(project: str, path: Path, findings: list[dict[str, object]]) -> None:
    raw = path.read_text(encoding="utf-8-sig", errors="replace")
    text = strip_comments(raw)
    allowed_colors = path.name in ALLOWED_COLOR_FILES

    if not allowed_colors:
        for match in HEX_RE.finditer(text):
            add_finding(findings, "direct_color", project, path, text, match.start(), match.group(0))
        for match in RGB_RE.finditer(text):
            add_finding(findings, "direct_color", project, path, text, match.start(), match.group(0))

    for match in IMPORTANT_RE.finditer(text):
        add_finding(findings, "important", project, path, text, match.start(), match.group(0))

    if path.suffix.lower() == ".razor":
        for match in INLINE_STYLE_RE.finditer(text):
            tag_end = text.find(">", match.end())
            tag_fragment = text[match.start():tag_end if tag_end >= 0 else match.end() + 240]
            if JUSTIFIED_DYNAMIC_STYLE_RE.search(tag_fragment):
                add_finding(
                    findings,
                    "inline_style_dynamic",
                    project,
                    path,
                    text,
                    match.start(),
                    tag_fragment,
                    "review",
                )
            else:
                add_finding(findings, "inline_style", project, path, text, match.start(), tag_fragment)
        for match in STYLE_BLOCK_RE.finditer(text):
            add_finding(findings, "style_block", project, path, text, match.start(), "<style>")
        for match in MANUAL_KPI_RE.finditer(text):
            tag = match.group(0)
            if "Navi" not in tag and not re.search(r"mobile-page[^\"]*kpi", tag, re.I):
                add_finding(findings, "manual_kpi", project, path, text, match.start(), tag)

        for match in ACTION_TAG_RE.finditer(text):
            tag_name = match.group(1).lower()
            tag = match.group(0)
            tone_match = COMPONENT_TONE_RE.search(tag) if "actionbutton" in tag_name else DATA_TONE_RE.search(tag)
            if not tone_match:
                add_finding(findings, "button_without_tone", project, path, text, match.start(), tag)
                continue
            tone = tone_match.group(1).strip().lower()
            if tone.startswith("@"):
                continue
            if tone not in CANONICAL_ACTION_TONES:
                add_finding(findings, "noncanonical_tone", project, path, text, match.start(), tag)

        for match in ACTION_BLOCK_RE.finditer(text):
            body = match.group("body")
            # Una etiqueta Razor calculada puede alternar entre dos intenciones.
            if "@(" in body or "@if" in body:
                continue
            strong = re.search(r"<strong\b[^>]*>(.*?)</strong\s*>", body, re.I | re.S)
            label_source = strong.group(1) if strong else body
            label = re.sub(r"<[^>]+>", " ", label_source)
            label = " ".join(label.split())
            expected = next((tone for pattern, tone in ACTION_WORD_TONES if pattern.search(label)), None)
            if expected is None:
                continue
            attrs = match.group("attrs")
            is_component = "actionbutton" in match.group("tag").lower()
            tone_match = COMPONENT_TONE_RE.search(attrs) if is_component else DATA_TONE_RE.search(attrs)
            if tone_match and not tone_match.group(1).strip().startswith("@"):
                actual = tone_match.group(1).strip().lower()
                if actual != expected:
                    add_finding(
                        findings,
                        "action_tone_mismatch",
                        project,
                        path,
                        text,
                        match.start(),
                        f"{label}: {actual} -> {expected}",
                    )

        kpi_cards = list(KPI_CARD_TAG_RE.finditer(text))
        for match in kpi_cards:
            tone_match = COMPONENT_TONE_RE.search(match.group(0))
            if not tone_match or tone_match.group(1).strip().startswith("@"):
                continue
            tone = tone_match.group(1).strip().lower()
            if tone not in CANONICAL_KPI_TONES:
                add_finding(findings, "noncanonical_kpi_tone", project, path, text, match.start(), match.group(0))

        if project == "Mobile" and len(kpi_cards) > 3 and "<NaviMobileKpiOverflow" not in text:
            add_finding(
                findings,
                "mobile_kpi_overflow_missing",
                project,
                path,
                text,
                kpi_cards[3].start(),
                f"{len(kpi_cards)} KPI sin NaviMobileKpiOverflow",
            )

        if project == "Mobile" and ROUTE_RE.search(text):
            routes = ROUTE_RE.findall(text)
            exempt = all(route in {"/login", "/logout", "/counter", "/weather"} for route in routes)
            if not exempt and not any(marker in text for marker in HEADER_MARKERS):
                add_finding(
                    findings,
                    "mobile_header_missing",
                    project,
                    path,
                    text,
                    0,
                    ", ".join(routes),
                )

    if path.suffix.lower() == ".css":
        is_view_css = path.name.endswith(".razor.css") or "/Pages/" in relative(path) or "/Components/Pages/" in relative(path)
        if is_view_css:
            for match in FONT_SIZE_RE.finditer(text):
                value = match.group(1).strip()
                if not re.fullmatch(r"var\(--(?:navi|ntx)-[\w-]+(?:\s*,[^)]*)?\)", value):
                    add_finding(findings, "view_font_size", project, path, text, match.start(), match.group(0))

        for match in OVERFLOW_HIDDEN_RE.finditer(text):
            add_finding(findings, "overflow_x_hidden", project, path, text, match.start(), match.group(0))
        for match in FIXED_HEIGHT_RE.finditer(text):
            add_finding(findings, "fixed_height", project, path, text, match.start(), match.group(0), "review")
        for match in BOOTSTRAP_COLOR_RE.finditer(text):
            add_finding(findings, "bootstrap_color", project, path, text, match.start(), match.group(0))

    if BOOTSTRAP_COLOR_RE.search(text) and path.suffix.lower() == ".razor":
        for match in BOOTSTRAP_COLOR_RE.finditer(text):
            add_finding(findings, "bootstrap_color", project, path, text, match.start(), match.group(0))

    if re.search(r"chart|donut|graph|gráfic", text, re.I) and not allowed_colors:
        for match in HEX_RE.finditer(text):
            add_finding(findings, "chart_direct_color", project, path, text, match.start(), match.group(0))


def duplicate_preference_assets(findings: list[dict[str, object]]) -> None:
    for project, path in (
        ("Admin", PROJECTS["Admin"] / "Components/App.razor"),
        ("Mobile", PROJECTS["Mobile"] / "wwwroot/index.html"),
    ):
        text = path.read_text(encoding="utf-8-sig", errors="replace")
        preference_names = (
            "navi-ui-preferences.js",
            "navi-ui-preferences.css",
        ) if project == "Admin" else (
            "navi-mobile-ui-preferences.js",
            "navi-mobile-ui-preferences.css",
        )
        for name in preference_names:
            count = text.count(name)
            if count > 1:
                add_finding(
                    findings,
                    "duplicate_preference_asset",
                    project,
                    path,
                    text,
                    text.find(name),
                    f"{name}: {count}",
                )


def write_reports(mode: str, findings: list[dict[str, object]]) -> tuple[Path, Path]:
    output_dir = ROOT / "docs/parametrizacion"
    output_dir.mkdir(parents=True, exist_ok=True)
    suffix = "BASELINE" if mode == "baseline" else "FINAL"
    json_path = output_dir / f"NAVI_UI_AUDIT_{suffix}.json"
    md_path = output_dir / f"NAVI_UI_AUDIT_{suffix}.md"
    counts = Counter(item["category"] for item in findings)
    by_project = Counter(item["project"] for item in findings)
    generated = datetime.now().astimezone().isoformat(timespec="seconds")
    blocking_count = sum(item["category"] in BLOCKING_CATEGORIES for item in findings)
    review_count = len(findings) - blocking_count
    report = {
        "generated_at": generated,
        "mode": mode,
        "root": str(ROOT),
        "canonical_action_tones": sorted(CANONICAL_ACTION_TONES),
        "canonical_kpi_tones": sorted(CANONICAL_KPI_TONES),
        "summary": {
            "total_findings": len(findings),
            "by_category": dict(sorted(counts.items())),
            "by_project": dict(sorted(by_project.items())),
            "blocking_findings": blocking_count,
            "review_findings": review_count,
            "strict_passed": blocking_count == 0,
        },
        "findings": findings,
    }
    json_path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")

    lines = [
        f"# Auditoría {'base' if mode == 'baseline' else 'final'} de apariencia NAVI",
        "",
        f"Generada: `{generated}`",
        "",
        "Esta auditoría analiza únicamente código fuente. No modifica archivos y no ejecuta las aplicaciones.",
        "",
        "## Resumen",
        "",
        f"- Hallazgos totales: **{len(findings)}**.",
        f"- Hallazgos bloqueantes: **{blocking_count}**.",
        f"- Observaciones de revisión: **{review_count}**.",
        f"- Resultado estricto: **{'APROBADO' if blocking_count == 0 else 'NO APROBADO'}**.",
    ]
    for project, count in sorted(by_project.items()):
        lines.append(f"- {project}: **{count}**.")
    lines.extend(["", "| Categoría | Hallazgos |", "|---|---:|"])
    for category, count in sorted(counts.items()):
        lines.append(f"| `{category}` | {count} |")
    lines.extend(["", "## Muestra de hallazgos", ""])
    for category in sorted(counts):
        items = [item for item in findings if item["category"] == category]
        lines.extend([f"### {category}", ""])
        for item in items[:25]:
            lines.append(
                f"- `{item['file']}:{item['line']}` — {item['snippet']}"
            )
        if len(items) > 25:
            lines.append(f"- … {len(items) - 25} adicionales en el JSON.")
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
            if path.is_file() and is_source(path):
                scan_file(project, path, findings)
    duplicate_preference_assets(findings)
    findings.sort(key=lambda item: (str(item["category"]), str(item["file"]), int(item["line"])))
    json_path, md_path = write_reports(args.mode, findings)
    summary = Counter(item["category"] for item in findings)
    print(json.dumps({"json": relative(json_path), "markdown": relative(md_path), "findings": len(findings), "by_category": dict(sorted(summary.items()))}, ensure_ascii=False, indent=2))

    return 1 if args.mode == "strict" and any(item["category"] in BLOCKING_CATEGORIES for item in findings) else 0


if __name__ == "__main__":
    raise SystemExit(main())

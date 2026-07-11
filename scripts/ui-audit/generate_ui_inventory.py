#!/usr/bin/env python3
"""Generate a reproducible visual inventory for NAVI Admin and Mobile."""

from __future__ import annotations

import argparse
import colorsys
import json
import re
from collections import Counter, defaultdict
from datetime import date
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
PROJECTS = {
    "admin": ROOT / "src/Navi.ToolsAssets.Admin",
    "mobile": ROOT / "src/Navi.ToolsAssets.MobilePwa",
}
APPROVED = {
    "#363534",
    "#C3CAC8",
    "#354550",
    "#EE2E2F",
    "#B2E100",
    "#EFF0C8",
    "#FFB301",
    "#FFFFFF",
}
TONES = {"primary", "positive", "danger", "warning", "secondary", "ghost"}
STYLE_BLOCK_RE = re.compile(r"<style\b[^>]*>(.*?)</style>", re.I | re.S)
ROUTE_RE = re.compile(r'^\s*@page\s+"([^"]+)"', re.M)
TAG_START_RE = re.compile(r"<(button|a|NaviActionButton|NaviMobileActionButton)\b", re.I)
CLASS_RE = re.compile(r"\bclass\s*=\s*(?:\"([^\"]*)\"|'([^']*)')", re.I | re.S)
TONE_RE = re.compile(r"\bTone\s*=\s*\"([^\"]+)\"", re.I)
HEX_RE = re.compile(r"(?<![\w-])#[0-9a-fA-F]{3,8}\b")
RGB_RE = re.compile(r"\brgba?\(([^)]*)\)", re.I)
HSL_RE = re.compile(r"\bhsla?\(([^)]*)\)", re.I)
GRADIENT_RE = re.compile(r"\b(?:linear|radial|conic)-gradient\s*\(", re.I)
IMPORTANT_RE = re.compile(r"!important\b", re.I)
LEGACY_COLOR_WORD_RE = re.compile(r"\b(?:purple|violet|indigo|blue)\b", re.I)
CHROMATIC_CLASS_WORD_RE = re.compile(
    r"(?:^|-)(?:red|green|yellow|orange|blue|cyan|purple|violet|indigo|gray|grey)(?:-|$)",
    re.I,
)
NAMED_COLOR_DECL_RE = re.compile(
    r"(?:^|[;{])\s*(?:color|background(?:-color)?|border(?:-[\w-]+)?-color|fill|stroke)\s*:\s*(black|white|gray|grey|red|green|yellow|blue|purple|violet|indigo)\s*(?:!important\s*)?(?:;|})",
    re.I | re.M,
)


def read_text(path: Path) -> tuple[str, bool]:
    raw = path.read_bytes()
    return raw.decode("utf-8-sig", errors="replace"), raw.startswith(b"\xef\xbb\xbf")


def strip_comments(text: str) -> str:
    text = re.sub(r"/\*.*?\*/", "", text, flags=re.S)
    return re.sub(r"@\*.*?\*@", "", text, flags=re.S)


def normalized_hex(token: str) -> tuple[str, str | None]:
    value = token.lstrip("#")
    if len(value) in (3, 4):
        value = "".join(char * 2 for char in value)
    if len(value) == 6:
        return f"#{value.upper()}", None
    if len(value) == 8:
        return f"#{value[:6].upper()}", value[6:].upper()
    return token.upper(), None


def channel(value: str) -> int:
    value = value.strip()
    if value.endswith("%"):
        return round(float(value[:-1]) * 2.55)
    return max(0, min(255, round(float(value))))


def functional_rgb(body: str) -> str | None:
    body = body.replace("/", " ")
    parts = [part for part in re.split(r"[\s,]+", body.strip()) if part]
    if len(parts) < 3:
        return None
    try:
        rgb = [channel(value) for value in parts[:3]]
    except ValueError:
        return None
    return "#" + "".join(f"{value:02X}" for value in rgb)


def functional_hsl(body: str) -> str | None:
    body = body.replace("/", " ")
    parts = [part for part in re.split(r"[\s,]+", body.strip()) if part]
    if len(parts) < 3 or not parts[1].endswith("%") or not parts[2].endswith("%"):
        return None
    try:
        hue = float(re.sub(r"(?:deg|rad|turn)$", "", parts[0])) % 360 / 360
        saturation = float(parts[1][:-1]) / 100
        lightness = float(parts[2][:-1]) / 100
    except ValueError:
        return None
    red, green, blue = colorsys.hls_to_rgb(hue, lightness, saturation)
    return "#" + "".join(f"{round(value * 255):02X}" for value in (red, green, blue))


def luminance(value: str) -> float:
    channels = [int(value[index:index + 2], 16) / 255 for index in (1, 3, 5)]
    converted = [item / 12.92 if item <= 0.04045 else ((item + 0.055) / 1.055) ** 2.4 for item in channels]
    return 0.2126 * converted[0] + 0.7152 * converted[1] + 0.0722 * converted[2]


def contrast(first: str, second: str) -> float:
    high, low = sorted((luminance(first), luminance(second)), reverse=True)
    return round((high + 0.05) / (low + 0.05), 2)


def rel(path: Path) -> str:
    return path.relative_to(ROOT).as_posix()


def class_value(tag: str) -> str:
    match = CLASS_RE.search(tag)
    return ((match.group(1) or match.group(2)) if match else "").strip()


def action_is_semantic(tag: str, path: Path) -> bool:
    tag_name = re.match(r"<([\w]+)", tag).group(1).lower()
    classes = class_value(tag)
    if tag_name in {"naviactionbutton", "navimobileactionbutton"}:
        tone = TONE_RE.search(tag)
        return bool(tone and tone.group(1).lower() in TONES)
    if path.name in {"NaviActionButton.razor", "NaviMobileActionButton.razor"} and "@CssClass" in classes:
        return True
    if re.search(r'\bdata-ntx-tone\s*=\s*"(?:primary|positive|danger|warning|secondary|ghost)"', tag, re.I):
        return True
    return bool(re.search(r"\bntx-btn\b", classes) and re.search(r"\bntx-btn--(?:primary|positive|danger|warning|secondary|ghost)\b", classes))


def consume_razor_group(text: str, start: int) -> int:
    depth = 0
    quote = None
    escape = False
    for index in range(start, len(text)):
        char = text[index]
        if quote:
            if escape:
                escape = False
            elif char == "\\":
                escape = True
            elif char == quote:
                quote = None
            continue
        if char in {'"', "'"}:
            quote = char
        elif char == "(":
            depth += 1
        elif char == ")":
            depth -= 1
            if depth == 0:
                return index + 1
    return len(text)


def iter_action_tags(text: str):
    position = 0
    while match := TAG_START_RE.search(text, position):
        quote = None
        index = match.end()
        while index < len(text):
            char = text[index]
            if quote:
                if text.startswith("@(", index):
                    index = consume_razor_group(text, index + 1)
                    continue
                if char == quote:
                    quote = None
            else:
                if char in {'"', "'"}:
                    quote = char
                elif char == ">":
                    index += 1
                    break
            index += 1
        yield match.group(1), match.start(), index, text[match.start():index]
        position = max(index, match.end())


def scan_color_source(name: str, kind: str, raw_text: str) -> dict:
    text = strip_comments(raw_text)
    colors: list[tuple[str, str, bool]] = []
    for match in HEX_RE.finditer(text):
        base, alpha = normalized_hex(match.group(0))
        colors.append((match.group(0), base, base in APPROVED))
    for match in RGB_RE.finditer(text):
        base = functional_rgb(match.group(1))
        colors.append((match.group(0), base or "UNPARSED", bool(base and base in APPROVED)))
    for match in HSL_RE.finditer(text):
        base = functional_hsl(match.group(1))
        colors.append((match.group(0), base or "UNPARSED", bool(base and base in APPROVED)))
    named = [match.group(1).lower() for match in NAMED_COLOR_DECL_RE.finditer(text)]
    bad = [item for item in colors if not item[2]]
    return {
        "source": name,
        "kind": kind,
        "chars": len(raw_text),
        "color_occurrences": len(colors),
        "unapproved_color_occurrences": len(bad),
        "unapproved_colors": dict(Counter(item[1] for item in bad).most_common()),
        "named_color_declarations": dict(Counter(named).most_common()),
        "gradients": len(GRADIENT_RE.findall(text)),
        "important": len(IMPORTANT_RE.findall(text)),
        "legacy_color_words": len(LEGACY_COLOR_WORD_RE.findall(text)),
    }


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--json", default="docs/visual-standard/point4/UI_INVENTORY_CORRECTED_20260710.json")
    parser.add_argument("--markdown", default="docs/visual-standard/point4/UI_INVENTORY_CORRECTED_20260710.md")
    parser.add_argument("--fail-on-violations", action="store_true")
    args = parser.parse_args()

    razor_files: list[tuple[str, Path]] = []
    for project, folder in PROJECTS.items():
        razor_files.extend((project, path) for path in folder.rglob("*.razor") if "bin" not in path.parts and "obj" not in path.parts)

    views = {"admin": [], "mobile": []}
    totals = Counter()
    style_files: list[str] = []
    action_combinations = Counter()
    unclassified_actions: list[dict] = []
    unclassified_action_count = 0
    static_classes = Counter()
    chromatic_classes = Counter()
    color_sources: list[dict] = []

    for project, path in sorted(razor_files, key=lambda item: rel(item[1])):
        text, had_bom = read_text(path)
        routes = ROUTE_RE.findall(text)
        style_blocks = STYLE_BLOCK_RE.findall(text)
        without_styles = STYLE_BLOCK_RE.sub("", text)
        inline_styles = re.findall(r'\bstyle\s*=\s*"([^"]*)"', without_styles, re.I | re.S)
        raw_buttons = len(re.findall(r"<button\b", text, re.I))
        anchors = len(re.findall(r"<a\b", text, re.I))
        blazor_inputs = len(re.findall(r"<Input(?:Text|TextArea|Select|Number|Date|Checkbox|Radio|RadioGroup)\b", text))
        values = {
            "raw_buttons": raw_buttons,
            "anchors": anchors,
            "tables": len(re.findall(r"<table\b", text, re.I)),
            "inputs": len(re.findall(r"<input\b", text, re.I)),
            "selects": len(re.findall(r"<select\b", text, re.I)),
            "textareas": len(re.findall(r"<textarea\b", text, re.I)),
            "blazor_inputs": blazor_inputs,
            "action_components": len(re.findall(r"<(?:NaviActionButton|NaviMobileActionButton)\b", text)),
            "onclick": len(re.findall(r"@onclick\b", text)),
            "style_blocks": len(style_blocks),
            "style_block_chars": sum(len(block) for block in style_blocks),
            "inline_style_attributes": len(re.findall(r"\bstyle\s*=", without_styles, re.I)),
            "static_inline_style_attributes": sum("@" not in value for value in inline_styles),
        }
        totals.update(values)
        totals[f"{project}_razor_files"] += 1
        totals[f"{project}_route_directives"] += len(routes)
        if had_bom:
            totals[f"{project}_bom_files"] += 1
        if style_blocks:
            style_files.append(rel(path))
        if routes:
            totals[f"{project}_routed_files"] += 1
            views[project].append({"file": rel(path), "routes": routes, "bom": had_bom, **values})

        for block_index, block in enumerate(style_blocks, 1):
            color_sources.append(scan_color_source(f"{rel(path)}::<style:{block_index}>", "razor-style", block))
        color_sources.append(scan_color_source(rel(path), "razor-markup", without_styles))

        for match in re.finditer(r"\bclass\s*=\s*\"([^\"]*)\"", text, re.I | re.S):
            for token in re.findall(r"(?<![@\w-])([A-Za-z_][\w-]*)", match.group(1)):
                static_classes[token] += 1
                if CHROMATIC_CLASS_WORD_RE.search(token):
                    chromatic_classes[token] += 1

        for tag_name, tag_start, tag_end, tag in iter_action_tags(text):
            name = tag_name.lower()
            classes = class_value(tag) or "<none>"
            is_action_anchor = name != "a" or bool(re.search(r"@onclick|\b(?:btn|action|button)\b", tag, re.I))
            if not is_action_anchor:
                continue
            action_combinations[classes] += 1
            if not action_is_semantic(tag, path):
                unclassified_action_count += 1
                if len(unclassified_actions) < 300:
                    line = text.count("\n", 0, tag_start) + 1
                    unclassified_actions.append({"file": rel(path), "line": line, "tag": name, "class": classes[:240]})

    css_files: list[Path] = []
    for folder in PROJECTS.values():
        css_files.extend(
            path for path in folder.rglob("*.css")
            if "bootstrap" not in path.parts and "bin" not in path.parts and "obj" not in path.parts
        )
    css_class_definitions = Counter()
    for path in sorted(css_files, key=rel):
        text, _ = read_text(path)
        color_sources.append(scan_color_source(rel(path), "css", text))
        totals["custom_css_file_chars"] += len(text)
        for token in re.findall(r"\.([A-Za-z_][\w-]*)", strip_comments(text)):
            css_class_definitions[token] += 1

    totals["razor_files"] = len(razor_files)
    totals["routed_files"] = totals["admin_routed_files"] + totals["mobile_routed_files"]
    totals["route_directives"] = totals["admin_route_directives"] + totals["mobile_route_directives"]
    totals["html_form_controls"] = totals["inputs"] + totals["selects"] + totals["textareas"]
    totals["all_form_controls"] = totals["html_form_controls"] + totals["blazor_inputs"]
    totals["custom_css_chars_including_style_blocks"] = totals["custom_css_file_chars"] + totals["style_block_chars"]
    totals["color_occurrences"] = sum(item["color_occurrences"] for item in color_sources)
    totals["unapproved_color_occurrences"] = sum(item["unapproved_color_occurrences"] for item in color_sources)
    totals["named_color_declarations"] = sum(sum(item["named_color_declarations"].values()) for item in color_sources)
    totals["gradients"] = sum(item["gradients"] for item in color_sources)
    totals["important"] = sum(item["important"] for item in color_sources)
    totals["legacy_color_words"] = sum(item["legacy_color_words"] for item in color_sources)
    totals["action_elements"] = sum(action_combinations.values())
    totals["unclassified_actions"] = unclassified_action_count
    totals["semantic_actions"] = totals["action_elements"] - totals["unclassified_actions"]
    totals["chromatic_class_occurrences"] = sum(chromatic_classes.values())

    unused_candidates = [
        {"class": name, "definitions": count}
        for name, count in css_class_definitions.most_common()
        if name not in static_classes
    ]
    contrast_pairs = []
    for role, foreground, background in (
        ("primary-label", "#FFFFFF", "#EE2E2F"),
        ("destructive-label", "#EE2E2F", "#FFFFFF"),
        ("positive-label", "#363534", "#B2E100"),
        ("warning-label", "#363534", "#FFB301"),
        ("table-header", "#FFFFFF", "#354550"),
        ("positive-as-text-on-white", "#B2E100", "#FFFFFF"),
        ("warning-as-text-on-white", "#FFB301", "#FFFFFF"),
    ):
        ratio = contrast(foreground, background)
        contrast_pairs.append({"role": role, "foreground": foreground, "background": background, "ratio": ratio, "aa_normal": ratio >= 4.5, "aa_large": ratio >= 3})

    report = {
        "generated_at": str(date.today()),
        "root": str(ROOT),
        "approved_palette": sorted(APPROVED),
        "summary": dict(sorted(totals.items())),
        "views": views,
        "style_block_files": style_files,
        "action_class_combinations": [{"class": name, "count": count} for name, count in action_combinations.most_common()],
        "unclassified_action_samples": unclassified_actions,
        "chromatic_classes": dict(chromatic_classes.most_common()),
        "contrast": contrast_pairs,
        "highest_risk_sources": sorted(color_sources, key=lambda item: (item["unapproved_color_occurrences"], item["important"], item["gradients"]), reverse=True)[:100],
        "unused_css_candidates": unused_candidates[:500],
    }

    json_path = ROOT / args.json
    md_path = ROOT / args.markdown
    json_path.parent.mkdir(parents=True, exist_ok=True)
    md_path.parent.mkdir(parents=True, exist_ok=True)
    json_path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")

    summary = report["summary"]
    markdown = f"""# Inventario UI corregido

Fecha: {report['generated_at']}

## Cobertura

- Razor: {summary['razor_files']} ({summary['admin_razor_files']} Admin, {summary['mobile_razor_files']} Mobile).
- Archivos enrutados: {summary['routed_files']} ({summary['admin_routed_files']} Admin, {summary['mobile_routed_files']} Mobile).
- Directivas `@page`: {summary['route_directives']}.
- Controles de formulario: {summary['all_form_controls']} ({summary['html_form_controls']} HTML y {summary['blazor_inputs']} Blazor `Input*`).
- Elementos de acción auditados: {summary['action_elements']}.
- Acciones sin clasificación canónica: {summary['unclassified_actions']}.

## Deuda visual

- Archivos con `<style>`: {len(style_files)}; bloques: {summary['style_blocks']}.
- CSS personalizado, incluidos bloques `<style>`: {summary['custom_css_chars_including_style_blocks']:,} caracteres.
- Colores no autorizados: {summary['unapproved_color_occurrences']:,} apariciones.
- Colores nombrados en declaraciones: {summary['named_color_declarations']:,}.
- Gradientes: {summary['gradients']:,}.
- `!important`: {summary['important']:,}.
- Referencias textuales legacy purple/blue/violet/indigo: {summary['legacy_color_words']:,}.
- Clases cromáticas no semánticas: {summary['chromatic_class_occurrences']:,}.
- Estilos inline estáticos: {summary['static_inline_style_attributes']:,}; los restantes son valores dinámicos de visualización.

## Contraste de la paleta

| Rol | Frente | Fondo | Ratio | AA normal | AA grande |
|---|---:|---:|---:|---:|---:|
"""
    for item in contrast_pairs:
        markdown += f"| {item['role']} | {item['foreground']} | {item['background']} | {item['ratio']}:1 | {'Sí' if item['aa_normal'] else 'No'} | {'Sí' if item['aa_large'] else 'No'} |\n"
    markdown += "\nEl JSON adjunto contiene vistas, rutas, fuentes de mayor riesgo, acciones sin clasificar y candidatos de CSS sin uso.\n"
    md_path.write_text(markdown, encoding="utf-8")

    print(json.dumps(summary, ensure_ascii=False, indent=2))
    violations = (
        summary["style_blocks"]
        + summary["unapproved_color_occurrences"]
        + summary["named_color_declarations"]
        + summary["gradients"]
        + summary["important"]
        + summary["unclassified_actions"]
        + summary["chromatic_class_occurrences"]
        + summary["static_inline_style_attributes"]
    )
    return 1 if args.fail_on_violations and violations else 0


if __name__ == "__main__":
    raise SystemExit(main())

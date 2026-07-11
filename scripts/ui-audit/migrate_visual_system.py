#!/usr/bin/env python3
"""Mechanical, behavior-preserving migration of NAVI visual sources."""

from __future__ import annotations

import math
import re
from pathlib import Path
import argparse
import subprocess


ROOT = Path(__file__).resolve().parents[2]
ADMIN = ROOT / "src/Navi.ToolsAssets.Admin"
MOBILE = ROOT / "src/Navi.ToolsAssets.MobilePwa"
STYLE_RE = re.compile(r"<style\b[^>]*>(.*?)</style>", re.I | re.S)
OPEN_ACTION_RE = re.compile(r"<(button|a)\b(?:(?:\"[^\"]*\"|'[^']*'|[^>])*)>", re.I | re.S)
OPEN_COMPONENT_RE = re.compile(r"<(NaviActionButton|NaviMobileActionButton)\b(?:(?:\"[^\"]*\"|'[^']*'|[^>])*)>", re.I | re.S)
CLASS_RE = re.compile(r"\bclass\s*=\s*(\"[^\"]*\"|'[^']*')", re.I | re.S)
HEX_RE = re.compile(r"(?<![\w-])#[0-9a-fA-F]{3,8}\b")
RGB_RE = re.compile(r"\brgba?\(([^)]*)\)", re.I)
HSL_RE = re.compile(r"\bhsla?\(([^)]*)\)", re.I)
DECL_RE = re.compile(r"(?P<lead>(?:^|[;{])[ \t\r\n]*)(?P<prop>--[\w-]+|[\w-]+)\s*:\s*(?P<value>[^;{}]*);", re.M | re.S)
PALETTE = {
    "black": "#363534",
    "clear": "#C3CAC8",
    "gray": "#354550",
    "red": "#EE2E2F",
    "green": "#B2E100",
    "beige": "#EFF0C8",
    "yellow": "#FFB301",
    "white": "#FFFFFF",
}
RGB_PALETTE = {name: tuple(int(value[index:index + 2], 16) for index in (1, 3, 5)) for name, value in PALETTE.items()}
VISUAL_PROPERTIES = {
    "color", "box-shadow", "text-shadow", "outline", "outline-color", "fill", "stroke", "opacity",
    "filter", "backdrop-filter", "clip-path", "mix-blend-mode", "accent-color", "caret-color",
    "text-decoration", "text-decoration-color", "text-transform", "letter-spacing", "line-height",
}


def read(path: Path) -> str:
    return path.read_bytes().decode("utf-8-sig", errors="replace")


def write(path: Path, text: str) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(text.rstrip() + "\n", encoding="utf-8")


def nearest_color(red: int, green: int, blue: int) -> str:
    def distance(candidate: tuple[int, int, int]) -> float:
        return math.sqrt(2 * (red - candidate[0]) ** 2 + 4 * (green - candidate[1]) ** 2 + 3 * (blue - candidate[2]) ** 2)
    name = min(RGB_PALETTE, key=lambda item: distance(RGB_PALETTE[item]))
    return PALETTE[name]


def normalize_hex(token: str) -> str:
    value = token[1:]
    if len(value) in (3, 4):
        value = "".join(char * 2 for char in value)
    if len(value) not in (6, 8):
        return token
    red, green, blue = (int(value[index:index + 2], 16) for index in (0, 2, 4))
    mapped = nearest_color(red, green, blue)
    return mapped + (value[6:].upper() if len(value) == 8 else "")


def channel(value: str) -> int:
    value = value.strip()
    return round(float(value[:-1]) * 2.55) if value.endswith("%") else max(0, min(255, round(float(value))))


def replace_rgb(match: re.Match[str]) -> str:
    body = match.group(1).replace("/", " ")
    parts = [item for item in re.split(r"[\s,]+", body.strip()) if item]
    if len(parts) < 3:
        return match.group(0)
    try:
        values = [channel(item) for item in parts[:3]]
    except ValueError:
        return match.group(0)
    mapped = nearest_color(*values)
    if match.group(0).lower().startswith("rgba") and len(parts) > 3:
        red, green, blue = RGB_PALETTE[next(name for name, value in PALETTE.items() if value == mapped)]
        return f"rgba({red}, {green}, {blue}, {parts[3]})"
    return mapped


def replace_colors(text: str) -> str:
    text = HEX_RE.sub(lambda match: normalize_hex(match.group(0)), text)
    text = RGB_RE.sub(replace_rgb, text)
    text = HSL_RE.sub(PALETTE["gray"], text)
    replacements = {
        "black": PALETTE["black"], "white": PALETTE["white"], "gray": PALETTE["gray"], "grey": PALETTE["gray"],
        "red": PALETTE["red"], "green": PALETTE["green"], "yellow": PALETTE["yellow"], "blue": PALETTE["gray"],
        "purple": PALETTE["red"], "violet": PALETTE["red"], "indigo": PALETTE["gray"],
    }
    declaration = re.compile(r"(?P<prefix>(?:color|background(?:-color)?|border(?:-[\w-]+)?-color|fill|stroke)\s*:\s*)(?P<name>" + "|".join(replacements) + r")\b", re.I)
    return declaration.sub(lambda match: match.group("prefix") + replacements[match.group("name").lower()], text)


def sanitize_layout_css(text: str) -> str:
    text = re.sub(r"/\*.*?\*/", "", text, flags=re.S)
    text = re.sub(r"\.blazor-error-boundary\s*\{base64,.*?\n\}", "", text, flags=re.S)
    text = text.replace("!important", "")

    def declaration(match: re.Match[str]) -> str:
        prop = match.group("prop").lower()
        value = match.group("value").strip()
        lead = match.group("lead")

        def preserved_lead() -> str:
            if "{" in lead:
                return lead[:lead.rfind("{") + 1]
            if ";" in lead:
                return lead[:lead.rfind(";") + 1]
            return lead

        if prop.startswith("--"):
            return preserved_lead()
        if (
            prop in VISUAL_PROPERTIES
            or prop.startswith("background")
            or prop.startswith("border")
            or prop.startswith("font")
            or prop.startswith("mask")
            or "gradient(" in value.lower()
            or HEX_RE.search(value)
            or RGB_RE.search(value)
            or HSL_RE.search(value)
        ):
            return preserved_lead()
        return f"{lead}{prop}: {value};"

    for _ in range(8):
        updated = DECL_RE.sub(declaration, text)
        if updated == text:
            break
        text = updated
    for _ in range(8):
        updated = re.sub(r"(?ms)([^{}]+)\{\s*\}", "", text)
        if updated == text:
            break
        text = updated
    text = re.sub(r"[ \t]+\n", "\n", text)
    text = re.sub(r"\n{3,}", "\n\n", text)
    return text.strip()


def tone_for(fragment: str) -> str:
    text = re.sub(r"<[^>]+>", " ", fragment).lower()
    if re.search(r"eliminar|quitar|rechazar|denegar|anular|desactivar|dar de baja|delete|remove|reject|deny", text):
        return "danger"
    if re.search(r"aprobar|aceptar|marcar conforme|completar|finalizar|conciliar|activar|approve|complete|finish", text):
        return "positive"
    if re.search(r"reintentar|reprogramar|pausar|aclarar|corregir|retry|pause|warning", text):
        return "warning"
    if re.search(r"guardar|crear|registrar|enviar|solicitar|programar|iniciar|empezar|continuar|confirmar|generar|asignar|importar|cargar|save|create|submit|send|start|confirm|generate|assign|upload", text):
        return "primary"
    if re.search(r"cancelar|cerrar|volver|regresar|limpiar|editar|ver|gestionar|filtrar|descargar|exportar|buscar|actualizar|cancel|close|back|clear|edit|view|manage|filter|download|export|search|refresh", text):
        return "secondary"
    return "ghost"


def add_class(tag: str, tone: str) -> str:
    canonical = f"ntx-btn ntx-btn--{tone}"
    if re.search(r"\bntx-btn--(?:primary|positive|danger|warning|secondary|ghost)\b", tag):
        return tag
    match = CLASS_RE.search(tag)
    if match:
        quoted = match.group(1)
        quote = quoted[0]
        current = quoted[1:-1].strip()
        replacement = f"{quote}{current} {canonical}{quote}" if current else f"{quote}{canonical}{quote}"
        return tag[:match.start(1)] + replacement + tag[match.end(1):]
    insertion = tag.rfind("/>")
    if insertion < 0:
        insertion = tag.rfind(">")
    return tag[:insertion].rstrip() + f' class="{canonical}" ' + tag[insertion:]


def migrate_actions(text: str) -> str:
    def raw_action(match: re.Match[str]) -> str:
        tag = match.group(0)
        name = match.group(1).lower()
        if name == "a" and not re.search(r"@onclick|\b(?:btn|action|button)\b", tag, re.I):
            return tag
        following = text[match.end():match.end() + 320]
        return add_class(tag, tone_for(tag + " " + following))

    text = OPEN_ACTION_RE.sub(raw_action, text)

    def component(match: re.Match[str]) -> str:
        tag = match.group(0)
        tone = tone_for(tag + " " + text[match.end():match.end() + 320])
        if re.search(r"\bTone\s*=", tag, re.I):
            tag = re.sub(r"\bTone\s*=\s*\"success\"", 'Tone="positive"', tag, flags=re.I)
            return tag
        insertion = tag.rfind("/>")
        if insertion < 0:
            insertion = tag.rfind(">")
        return tag[:insertion].rstrip() + f' Tone="{tone}" ' + tag[insertion:]

    return OPEN_COMPONENT_RE.sub(component, text)


def rename_legacy_chromatic_tokens(text: str) -> str:
    text = re.sub(r"\bpurple\b", "primary", text, flags=re.I)
    text = re.sub(r"\bviolet\b", "primary", text, flags=re.I)
    text = re.sub(r"\bblue\b", "neutral", text, flags=re.I)
    return re.sub(r"\bindigo\b", "neutral", text, flags=re.I)


def migrate_razor(path: Path) -> None:
    text = read(path)
    blocks = STYLE_RE.findall(text)
    if blocks:
        isolated = path.with_suffix(path.suffix + ".css")
        existing = read(isolated) if isolated.exists() else ""
        combined = "\n\n".join([existing, *blocks])
        sanitized = sanitize_layout_css(combined)
        if sanitized:
            write(isolated, sanitized)
        text = STYLE_RE.sub("", text)
    text = migrate_actions(text)
    text = replace_colors(text)
    text = rename_legacy_chromatic_tokens(text)
    write(path, text)


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--sanitize-existing", action="store_true")
    parser.add_argument("--rebuild-from-snapshot")
    parser.add_argument("--restore-razor-from-snapshot")
    args = parser.parse_args()

    if args.restore_razor_from_snapshot:
        commit = args.restore_razor_from_snapshot
        for folder in (ADMIN, MOBILE):
            for razor in sorted(folder.rglob("*.razor")):
                if "bin" in razor.parts or "obj" in razor.parts:
                    continue
                result = subprocess.run(
                    ["git", "show", f"{commit}:{razor.relative_to(ROOT).as_posix()}"],
                    cwd=ROOT,
                    check=False,
                    capture_output=True,
                )
                if result.returncode != 0:
                    continue
                text = result.stdout.decode("utf-8-sig", errors="replace")
                text = STYLE_RE.sub("", text).replace("!important", "")
                lines = []
                for line in text.splitlines():
                    if "return" in line and "gradient(" in line:
                        lines.append(line[:len(line) - len(line.lstrip())] + 'return "background: #FFFFFF;";')
                    else:
                        lines.append(line)
                write(razor, rename_legacy_chromatic_tokens(replace_colors("\n".join(lines))))
        print(f"Razor sources restored safely from {commit}")
        return

    if args.rebuild_from_snapshot:
        commit = args.rebuild_from_snapshot

        def snapshot_text(path: Path) -> str:
            result = subprocess.run(
                ["git", "show", f"{commit}:{path.relative_to(ROOT).as_posix()}"],
                cwd=ROOT,
                check=False,
                capture_output=True,
            )
            return result.stdout.decode("utf-8-sig", errors="replace") if result.returncode == 0 else ""

        admin_source = snapshot_text(ADMIN / "wwwroot/app.css")
        mobile_source = snapshot_text(MOBILE / "wwwroot/css/app.css")
        mobile_detail_source = snapshot_text(MOBILE / "wwwroot/css/mobile-physical-detail.css")
        write(ADMIN / "wwwroot/css/ntx/layouts-admin.css", rename_legacy_chromatic_tokens(sanitize_layout_css(admin_source)))
        write(MOBILE / "wwwroot/css/ntx/layouts-mobile.css", rename_legacy_chromatic_tokens(sanitize_layout_css(mobile_source + "\n\n" + mobile_detail_source)))

        rebuilt: set[Path] = set()
        for folder in (ADMIN, MOBILE):
            for razor in sorted(folder.rglob("*.razor")):
                if "bin" in razor.parts or "obj" in razor.parts:
                    continue
                original = snapshot_text(razor)
                isolated = razor.with_suffix(razor.suffix + ".css")
                original_isolated = snapshot_text(isolated)
                blocks = STYLE_RE.findall(original)
                if original_isolated or blocks:
                    combined = "\n\n".join([original_isolated, *blocks])
                    sanitized = rename_legacy_chromatic_tokens(sanitize_layout_css(combined))
                    if sanitized:
                        write(isolated, sanitized)
                        rebuilt.add(isolated)
        for folder in (ADMIN, MOBILE):
            for isolated in folder.rglob("*.razor.css"):
                if isolated not in rebuilt and not snapshot_text(isolated):
                    isolated.unlink()
        print(f"Layout sources rebuilt from {commit}")
        return

    if args.sanitize_existing:
        for folder in (ADMIN, MOBILE):
            for path in sorted(folder.rglob("*.css")):
                if "bootstrap" in path.parts or path.name in {"tokens.css", "base.css", "components.css", "utilities.css", "app.css"}:
                    continue
                if path.name.endswith(".razor.css") or path.name.startswith("layouts-"):
                    write(path, rename_legacy_chromatic_tokens(sanitize_layout_css(read(path))))
            for path in sorted(folder.rglob("*.razor")):
                text = read(path).replace("!important", "")
                lines = []
                for line in text.splitlines():
                    if "return" in line and "gradient(" in line:
                        lines.append(line[:len(line) - len(line.lstrip())] + 'return "background: #FFFFFF;";')
                    else:
                        lines.append(line)
                write(path, rename_legacy_chromatic_tokens(replace_colors("\n".join(lines))))
        print("Existing visual sources sanitized")
        return

    admin_app = ADMIN / "wwwroot/app.css"
    mobile_app = MOBILE / "wwwroot/css/app.css"
    mobile_detail = MOBILE / "wwwroot/css/mobile-physical-detail.css"
    admin_layout = sanitize_layout_css(read(admin_app))
    mobile_layout = sanitize_layout_css(read(mobile_app) + "\n\n" + (read(mobile_detail) if mobile_detail.exists() else ""))
    write(ADMIN / "wwwroot/css/ntx/layouts-admin.css", admin_layout)
    write(MOBILE / "wwwroot/css/ntx/layouts-mobile.css", mobile_layout)

    for folder in (ADMIN, MOBILE):
        for path in sorted(folder.rglob("*.razor")):
            if "bin" not in path.parts and "obj" not in path.parts:
                migrate_razor(path)

    for folder in (ADMIN, MOBILE):
        for path in sorted(folder.rglob("*.razor.css")):
            write(path, rename_legacy_chromatic_tokens(sanitize_layout_css(read(path))))

    write(admin_app, "/* Host-only styles live in the ntx layered architecture. */")
    write(mobile_app, "/* Host-only styles live in the ntx layered architecture. */")

    for obsolete in (
        ADMIN / "wwwroot/brand-navitrans.css",
        MOBILE / "wwwroot/css/brand-navitrans.css",
        mobile_detail,
    ):
        if obsolete.exists():
            obsolete.unlink()

    print("Visual source migration completed")


if __name__ == "__main__":
    main()

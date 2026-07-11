#!/usr/bin/env python3
"""Add semantic action tones without modifying Razor class expressions."""

from __future__ import annotations

import re
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
FOLDERS = (ROOT / "src/Navi.ToolsAssets.Admin", ROOT / "src/Navi.ToolsAssets.MobilePwa")
START_RE = re.compile(r"<(button|a|NaviActionButton|NaviMobileActionButton)\b", re.I)


def consume_group(text: str, start: int) -> int:
    depth = 0
    quote: str | None = None
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


def tag_end(text: str, start: int) -> int:
    quote: str | None = None
    index = start
    while index < len(text):
        char = text[index]
        if quote:
            if text.startswith("@(", index):
                index = consume_group(text, index + 1)
                continue
            if char == quote:
                quote = None
        else:
            if char in {'"', "'"}:
                quote = char
            elif char == ">":
                return index + 1
        index += 1
    return len(text)


def iter_tags(text: str):
    position = 0
    while match := START_RE.search(text, position):
        end = tag_end(text, match.start())
        yield match.group(1), match.start(), end, text[match.start():end]
        position = max(end, match.end())


def tone_for(fragment: str) -> str:
    value = re.sub(r"<[^>]+>", " ", fragment).lower()
    if re.search(r"eliminar|quitar|rechazar|denegar|anular|desactivar|dar de baja|delete|remove|reject|deny", value):
        return "danger"
    if re.search(r"aprobar|aceptar|marcar conforme|completar|finalizar|conciliar|activar|approve|complete|finish", value):
        return "positive"
    if re.search(r"reintentar|reprogramar|pausar|aclarar|corregir|retry|pause|warning", value):
        return "warning"
    if re.search(r"guardar|crear|registrar|enviar|solicitar|programar|iniciar|empezar|continuar|confirmar|generar|asignar|importar|cargar|save|create|submit|send|start|confirm|generate|assign|upload", value):
        return "primary"
    if re.search(r"cancelar|cerrar|volver|regresar|limpiar|editar|ver|gestionar|filtrar|descargar|exportar|buscar|actualizar|cancel|close|back|clear|edit|view|manage|filter|download|export|search|refresh", value):
        return "secondary"
    return "ghost"


def insert_attribute(tag: str, attribute: str) -> str:
    index = tag.rfind("/>")
    if index < 0:
        index = tag.rfind(">")
    return tag[:index].rstrip() + " " + attribute + " " + tag[index:]


def classify(text: str) -> str:
    replacements: list[tuple[int, int, str]] = []
    for name, start, end, tag in iter_tags(text):
        lowered = name.lower()
        following = text[end:end + 360]
        tone = tone_for(tag + " " + following)
        if lowered == "a" and not re.search(r"@onclick|\b(?:btn|action|button)\b", tag, re.I):
            continue
        if lowered in {"naviactionbutton", "navimobileactionbutton"}:
            if re.search(r"\bTone\s*=", tag, re.I):
                updated = re.sub(r"\bTone\s*=\s*\"success\"", 'Tone="positive"', tag, flags=re.I)
            else:
                updated = insert_attribute(tag, f'Tone="{tone}"')
        elif re.search(r"\bdata-ntx-tone\s*=", tag, re.I):
            updated = tag
        else:
            updated = insert_attribute(tag, f'data-ntx-tone="{tone}"')
        if updated != tag:
            replacements.append((start, end, updated))
    for start, end, replacement in reversed(replacements):
        text = text[:start] + replacement + text[end:]
    return text


def main() -> None:
    changed = 0
    for folder in FOLDERS:
        for path in sorted(folder.rglob("*.razor")):
            if "bin" in path.parts or "obj" in path.parts or path.name in {"NaviActionButton.razor", "NaviMobileActionButton.razor"}:
                continue
            original = path.read_text(encoding="utf-8-sig")
            updated = classify(original)
            if updated != original:
                path.write_text(updated.rstrip() + "\n", encoding="utf-8")
                changed += 1
    print(f"Semantic action tones updated in {changed} Razor files")


if __name__ == "__main__":
    main()

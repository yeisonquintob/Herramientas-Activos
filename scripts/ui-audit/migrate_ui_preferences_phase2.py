#!/usr/bin/env python3
"""Mechanical, presentation-only migration to the NAVI appearance contract.

The script deliberately limits itself to Razor/CSS/host JavaScript. It does not
change C# services, API calls, routes, permissions, bindings or event names.
"""

from __future__ import annotations

import re
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
PROJECTS = (
    ROOT / "src/Navi.ToolsAssets.Admin",
    ROOT / "src/Navi.ToolsAssets.MobilePwa",
)
EXCLUDED = {"bin", "obj", ".git", "export_codigo", "backup", "backups"}
ALLOWED_COLORS = {
    "tokens.css",
    "navi-ui-preferences.css",
    "navi-mobile-ui-preferences.css",
    "SettingsAppearance.razor.css",
    "MobileAppearanceSettingsPanel.razor.css",
}

COLOR_MAP = {
    "#354550": "var(--navi-color-institutional)",
    "#C3CAC8": "var(--navi-color-border)",
    "#EE2E2F": "var(--navi-action-danger)",
    "#363534": "var(--navi-color-text)",
    "#675579": "var(--navi-action-operation)",
    "#EFF0C8": "var(--navi-action-affirmative-soft)",
    "#5F7048": "var(--navi-action-affirmative-hover)",
    "#708354": "var(--navi-action-affirmative)",
    "#FFB301": "var(--navi-action-warning)",
    "#B2E100": "var(--navi-action-affirmative)",
    "#584868": "var(--navi-action-operation-hover)",
    "#C97828": "var(--navi-action-warning)",
    "#66737A": "var(--navi-color-muted)",
    "#657C8C": "var(--navi-action-consult)",
    "#A85F21": "var(--navi-action-warning-hover)",
    "#F7F8F8": "var(--navi-color-surface-soft)",
    "#A8B797": "color-mix(in srgb, var(--navi-action-affirmative) 55%, var(--navi-color-surface))",
    "#D7DCDB": "var(--navi-color-border)",
    "#C92324": "var(--navi-action-danger-hover)",
    "#F6F7F7": "var(--navi-color-surface-soft)",
    "#F7F4F8": "var(--navi-action-operation-soft)",
    "#DE8D8E": "color-mix(in srgb, var(--navi-action-danger) 48%, var(--navi-color-surface))",
    "#F5F6F6": "var(--navi-color-surface-soft)",
    "#F3EFF6": "var(--navi-action-operation-soft)",
    "#263640": "var(--navi-action-secondary-hover)",
    "#9A8AA7": "color-mix(in srgb, var(--navi-action-operation) 60%, var(--navi-color-surface))",
    "#D5A16D": "color-mix(in srgb, var(--navi-action-warning) 62%, var(--navi-color-surface))",
    "#F5F1F7": "var(--navi-action-operation-soft)",
    "#FFF8E8": "var(--navi-action-warning-soft)",
    "#FFF5F5": "var(--navi-action-danger-soft)",
    "#E1E5E4": "var(--navi-color-border)",
    "#F3F5F5": "var(--navi-color-surface-soft)",
    "#E3E7E6": "var(--navi-color-border)",
    "#F8F7F9": "var(--navi-action-operation-soft)",
    "#9EA8A6": "var(--navi-color-muted)",
    "#FFF8E6": "var(--navi-action-warning-soft)",
    "#F4F5F5": "var(--navi-color-surface-soft)",
}

BOOTSTRAP_CLASSES = {
    "btn-outline-primary": "ntx-btn--operation",
    "btn-outline-success": "ntx-btn--affirmative",
    "btn-outline-info": "ntx-btn--consult",
    "btn-outline-warning": "ntx-btn--warning",
    "btn-outline-danger": "ntx-btn--danger",
    "btn-outline-dark": "ntx-btn--secondary",
    "btn-primary": "ntx-btn--operation",
    "btn-success": "ntx-btn--affirmative",
    "btn-info": "ntx-btn--consult",
    "btn-warning": "ntx-btn--warning",
    "btn-danger": "ntx-btn--danger",
    "btn-dark": "ntx-btn--secondary",
    "text-primary": "navi-text-operation",
    "text-success": "navi-text-affirmative",
    "text-info": "navi-text-consult",
    "text-warning": "navi-text-warning",
    "text-danger": "navi-text-danger",
    "text-dark": "navi-text",
    "text-bg-primary": "navi-bg-operation",
    "text-bg-success": "navi-bg-affirmative",
    "text-bg-info": "navi-bg-consult",
    "text-bg-warning": "navi-bg-warning",
    "text-bg-danger": "navi-bg-danger",
    "text-bg-dark": "navi-bg-secondary",
    "badge-primary": "navi-status-operation",
    "badge-success": "navi-status-positive",
    "badge-info": "navi-status-consult",
    "badge-warning": "navi-status-warning",
    "badge-danger": "navi-status-danger",
    "badge-dark": "navi-status-neutral",
    "bg-primary": "navi-bg-operation",
    "bg-success": "navi-bg-affirmative",
    "bg-info": "navi-bg-consult",
    "bg-warning": "navi-bg-warning",
    "bg-danger": "navi-bg-danger",
    "bg-dark": "navi-bg-secondary",
    "border-primary": "navi-border-operation",
    "border-success": "navi-border-affirmative",
    "border-info": "navi-border-consult",
    "border-warning": "navi-border-warning",
    "border-danger": "navi-border-danger",
    "border-dark": "navi-border-secondary",
}

BS_VARIABLES = {
    "primary": "var(--navi-action-operation)",
    "success": "var(--navi-action-affirmative)",
    "info": "var(--navi-action-consult)",
    "warning": "var(--navi-action-warning)",
    "danger": "var(--navi-action-danger)",
    "dark": "var(--navi-color-text)",
}

ALIASES = {
    "success": "affirmative",
    "positive": "affirmative",
    "approve": "affirmative",
    "approved": "affirmative",
    "accept": "affirmative",
    "confirm": "affirmative",
    "primary": "operation",
    "edit": "operation",
    "assign": "operation",
    "manage": "operation",
    "create": "operation",
    "destructive": "danger",
    "delete": "danger",
    "remove": "danger",
    "reject": "danger",
    "deny": "danger",
    "cancel-danger": "danger",
    "alert": "warning",
    "caution": "warning",
    "outline": "secondary",
    "back": "secondary",
    "cancel": "secondary",
    "link": "consult",
    "detail": "consult",
    "view": "consult",
}


def is_source(path: Path) -> bool:
    return (
        path.is_file()
        and not any(part in EXCLUDED for part in path.parts)
        and not path.name.endswith((".min.css", ".min.js", ".styles.css"))
        and path.suffix.lower() in {".css", ".razor", ".js", ".html"}
    )


def classify_action(context: str) -> str:
    source = context.lower()
    groups = (
        ("danger", ("rechaz", "deneg", "elimin", "borrar", "anular", "dar de baja", "desactiv", "delete", "remove", "cancel-danger")),
        ("refresh", ("actualiz", "recarg", "refresh", "reload", "sincroniz")),
        ("affirmative", ("guardar", "confirm", "validar", "aprobar", "approve", "aceptar", "accept", "enviar", "finaliz", "finish", "closephysical", "save", "submit", "iniciar sesión")),
        ("secondary", ("volver", "atrás", "atras", "limpiar", "deshacer", "cancelar", "cerrar modal", "goback")),
        ("warning", ("regresar", "devolver", "reabrir", "retomar", "return")),
        ("consult", ("consult", "detalle", "revisar", "export", "descargar", "imprimir", "download", "print", "view")),
        ("expand", ("mostrar", "ocultar", "expand", "contraer", "toggle", "collapse", "accordion")),
        ("operation", ("editar", "edit", "asign", "gestionar", "crear", "solicitar", "reportar", "nuevo", "agregar", "cambiar", "abrir", "continuar", "procesar", "quotation", "cotiz", "start")),
        ("ghost", ("menu", "menú", "notification", "campana", "close", "dismiss", "avatar", "pagination", "pagin")),
    )
    for tone, words in groups:
        if any(word in source for word in words):
            return tone
    return "ghost"


def migrate_buttons(text: str) -> str:
    # Repara únicamente la secuencia que produjo la primera versión del
    # migrador al confundir el `>` de una lambda Razor con el cierre del tag.
    text = re.sub(
        r'=\s+data-ntx-tone="[^"]+">\s*',
        "=> ",
        text,
        flags=re.I,
    )

    # Un `>` dentro de un atributo entre comillas no cierra la etiqueta.
    tag_re = re.compile(
        r"<button\b(?:[^>\"']|\"[^\"]*\"|'[^']*')*>",
        re.I | re.S,
    )

    def set_button_tone(tag: str, context: str) -> str:
        tone = classify_action(context)
        existing = re.search(r'data-ntx-tone\s*=\s*"([^"]+)"', tag, re.I)
        if existing:
            value = existing.group(1).strip().lower()
            if value.startswith("@"):
                return tag
            canonical = ALIASES.get(value, value)
            if canonical == "operation" and value == "primary":
                canonical = tone if tone != "ghost" else "operation"
            return tag[:existing.start(1)] + tone + tag[existing.end(1):]
        return tag[:-1].rstrip() + f' data-ntx-tone="{tone}">'

    full_button_re = re.compile(
        r"(<button\b(?:[^>\"']|\"[^\"]*\"|'[^']*')*>)(.*?)(</button>)",
        re.I | re.S,
    )

    def full_button(match: re.Match[str]) -> str:
        tag, body, closing = match.groups()
        context = tag + " " + re.sub(r"<[^>]+>", " ", body)
        return set_button_tone(tag, context) + body + closing

    text = full_button_re.sub(full_button, text)

    def button(match: re.Match[str]) -> str:
        tag = match.group(0)
        if re.search(r'data-ntx-tone\s*=', tag, re.I):
            return tag
        return set_button_tone(tag, tag)

    text = tag_re.sub(button, text)

    def component_tone(match: re.Match[str]) -> str:
        value = match.group(2).strip().lower()
        return match.group(1) + ALIASES.get(value, value) + match.group(3)

    text = re.sub(
        r'(\bTone\s*=\s*")([^"@]+)(")',
        component_tone,
        text,
        flags=re.I,
    )
    return text


def font_token(selector: str, mobile: bool) -> str:
    source = selector.lower()
    prefix = "--navi-mobile-font-" if mobile else "--navi-font-"
    if "kpi" in source and any(word in source for word in ("value", "number", "amount", "count", "> b", "> strong")):
        return f"var({prefix}kpi-value)"
    if "kpi" in source and any(word in source for word in ("description", "subtitle", "small", "detail")):
        return f"var({prefix}kpi-description)"
    if "kpi" in source:
        return f"var({prefix}kpi-title)"
    if any(word in source for word in ("button", ".btn", "action", "toggle", "tab")):
        return f"var({prefix}button)"
    if any(word in source for word in ("input", "select", "textarea", "form-control")):
        return f"var({prefix}input)"
    if "label" in source:
        return f"var({prefix}label)"
    if any(word in source for word in ("badge", "status", "chip", "pill", "eyebrow", "counter")):
        return f"var({prefix}badge)"
    if any(word in source for word in ("small", "help", "hint", "meta", "description", "subtitle", "caption", "empty")):
        return f"var({prefix}help)"
    if mobile and any(word in source for word in ("banner", "shell-header", "page-header", "hero h1")):
        return "var(--navi-mobile-font-header-title)"
    if not mobile and any(word in source for word in ("page-title", "hero h1", "banner h1")):
        return "var(--navi-font-page-title)"
    if any(word in source for word in ("panel", "modal-title", " h2", " h3")):
        return f"var({prefix}panel-title)"
    if "title" in source or "card" in source:
        return f"var({prefix}card-title)"
    if not mobile and (" th" in source or source.strip().endswith("th")):
        return "var(--navi-font-table-header)"
    if not mobile and any(word in source for word in ("table", " td")):
        return "var(--navi-font-table)"
    return f"var({prefix}body)"


def migrate_font_sizes(text: str, mobile: bool) -> str:
    block_re = re.compile(r"([^{}]+)\{([^{}]*)\}", re.S)

    def block(match: re.Match[str]) -> str:
        selector, body = match.group(1), match.group(2)
        token = font_token(selector, mobile)
        body = re.sub(r"font-size\s*:\s*[^;}]+", f"font-size: {token}", body, flags=re.I)
        return selector + "{" + body + "}"

    return block_re.sub(block, text)


def migrate_colors(text: str) -> str:
    # White text/fill/stroke on solid actions must stay white in dark mode.
    text = re.sub(
        r"((?<![-\w])(?:color|fill|stroke)\s*:\s*)#(?:fff|ffffff)\b",
        r"\1var(--navi-color-on-solid)",
        text,
        flags=re.I,
    )
    text = re.sub(
        r'((?:fill|stroke)\s*=\s*["\'])#(?:fff|ffffff)\b',
        r"\1var(--navi-color-on-solid)",
        text,
        flags=re.I,
    )
    text = re.sub(r"#(?:fff|ffffff)\b", "var(--navi-color-surface)", text, flags=re.I)
    for source, target in COLOR_MAP.items():
        text = re.sub(re.escape(source) + r"\b", target, text, flags=re.I)
    text = re.sub(
        r"rgba?\(\s*53\s*[, ]\s*69\s*[, ]\s*80\s*(?:,|/)\s*[^)]+\)",
        "var(--navi-shadow-color)",
        text,
        flags=re.I,
    )
    text = re.sub(
        r"rgba?\(\s*195\s*[, ]\s*202\s*[, ]\s*200\s*(?:,|/)\s*[^)]+\)",
        "color-mix(in srgb, var(--navi-color-border) 35%, transparent)",
        text,
        flags=re.I,
    )
    return text


def migrate_bootstrap(text: str) -> str:
    for source, target in BOOTSTRAP_CLASSES.items():
        text = re.sub(
            rf"(?<!navi-)(?<!ntx-)\b{re.escape(source)}\b",
            target,
            text,
            flags=re.I,
        )
    for name, target in BS_VARIABLES.items():
        text = re.sub(rf"var\(--bs-{name}\)", target, text, flags=re.I)
    return text


def migrate(path: Path) -> bool:
    original = path.read_text(encoding="utf-8-sig", errors="replace")
    text = original
    if path.name not in ALLOWED_COLORS:
        text = migrate_colors(text)
    text = migrate_bootstrap(text)
    if path.suffix.lower() == ".css":
        text = migrate_font_sizes(
            text,
            "Navi.ToolsAssets.MobilePwa" in path.parts,
        )
        text = re.sub(r"\s*!important\b", "", text, flags=re.I)
        text = re.sub(r"overflow-x\s*:\s*hidden", "overflow-x: clip", text, flags=re.I)
    if path.suffix.lower() == ".razor":
        text = migrate_buttons(text)
    if text == original:
        return False
    path.write_text(text, encoding="utf-8")
    return True


def main() -> int:
    changed: list[str] = []
    for project in PROJECTS:
        for path in sorted(project.rglob("*")):
            if is_source(path) and migrate(path):
                changed.append(path.relative_to(ROOT).as_posix())
    print(f"Archivos migrados: {len(changed)}")
    for path in changed:
        print(path)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

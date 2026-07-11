#!/usr/bin/env python3
"""Validate routed Razor and custom CSS before browser viewport checks."""

from __future__ import annotations

import json
import re
from collections import Counter
from datetime import date
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
INVENTORY = ROOT / "docs/visual-standard/point4/UI_INVENTORY_CORRECTED_20260710.json"
OUT_JSON = ROOT / "docs/visual-standard/point4/UI_STRUCTURE_VALIDATION_20260710.json"
OUT_MD = ROOT / "docs/visual-standard/point4/UI_STRUCTURE_VALIDATION_20260710.md"
VIEWPORTS = (360, 390, 768, 1366, 1920)
STYLE_BLOCK_RE = re.compile(r"<style\b", re.I)
STATIC_STYLE_RE = re.compile(r'\bstyle\s*=\s*"(?![^"]*@)[^"]*"', re.I | re.S)


def css_balance(text: str) -> tuple[bool, str]:
    depth = 0
    quote = None
    escape = False
    index = 0
    while index < len(text):
        if text.startswith("/*", index) and quote is None:
            end = text.find("*/", index + 2)
            if end < 0:
                return False, "comentario CSS sin cierre"
            index = end + 2
            continue
        char = text[index]
        if quote:
            if escape:
                escape = False
            elif char == "\\":
                escape = True
            elif char == quote:
                quote = None
        elif char in {'"', "'"}:
            quote = char
        elif char == "{":
            depth += 1
        elif char == "}":
            depth -= 1
            if depth < 0:
                return False, f"cierre inesperado en carácter {index}"
        index += 1
    if quote:
        return False, "cadena CSS sin cierre"
    if depth:
        return False, f"{depth} bloque(s) CSS sin cierre"
    return True, "ok"


def main() -> int:
    inventory = json.loads(INVENTORY.read_text(encoding="utf-8"))
    css_results = []
    for project in ("Navi.ToolsAssets.Admin", "Navi.ToolsAssets.MobilePwa"):
        folder = ROOT / "src" / project
        for path in folder.rglob("*.css"):
            if any(part in {"bin", "obj", "bootstrap"} for part in path.parts):
                continue
            valid, detail = css_balance(path.read_text(encoding="utf-8-sig"))
            css_results.append({"file": path.relative_to(ROOT).as_posix(), "valid": valid, "detail": detail})

    matrix = []
    failures = []
    by_app = Counter()
    for app, views in inventory["views"].items():
        for view in views:
            path = ROOT / view["file"]
            text = path.read_text(encoding="utf-8-sig")
            structural_errors = []
            if not view["routes"]:
                structural_errors.append("sin ruta")
            if STYLE_BLOCK_RE.search(text):
                structural_errors.append("contiene <style>")
            if STATIC_STYLE_RE.search(text):
                structural_errors.append("contiene estilo inline estático")
            for width in VIEWPORTS:
                status = "ready_for_runtime" if not structural_errors else "failed"
                row = {
                    "app": app,
                    "file": view["file"],
                    "routes": view["routes"],
                    "width": width,
                    "status": status,
                    "errors": structural_errors,
                }
                matrix.append(row)
                by_app[f"{app}_{status}"] += 1
                if structural_errors:
                    failures.append(row)

    invalid_css = [item for item in css_results if not item["valid"]]
    result = {
        "generated_at": str(date.today()),
        "view_files": sum(len(items) for items in inventory["views"].values()),
        "viewports": list(VIEWPORTS),
        "matrix_checks": len(matrix),
        "runtime_status": "blocked_by_environment_port_permission",
        "structural_failures": len(failures),
        "css_files": len(css_results),
        "invalid_css_files": invalid_css,
        "counts": dict(sorted(by_app.items())),
        "matrix": matrix,
    }
    OUT_JSON.write_text(json.dumps(result, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    OUT_MD.write_text(
        "# Validación estructural de vistas\n\n"
        f"Fecha: {result['generated_at']}\n\n"
        f"- Vistas enrutadas: {result['view_files']}.\n"
        f"- Anchos preparados: {', '.join(map(str, VIEWPORTS))} px.\n"
        f"- Combinaciones vista/ancho comprobadas estructuralmente: {result['matrix_checks']}.\n"
        f"- CSS personalizado analizado: {result['css_files']} archivos; inválidos: {len(invalid_css)}.\n"
        f"- Fallos estructurales: {len(failures)}.\n\n"
        "La ejecución visual en navegador queda separada de esta comprobación: el entorno bloqueó el permiso "
        "para abrir los puertos localhost. Las filas `ready_for_runtime` no se declaran como validación visual ejecutada.\n",
        encoding="utf-8",
    )
    print(json.dumps({key: value for key, value in result.items() if key != "matrix"}, ensure_ascii=False, indent=2))
    return 1 if failures or invalid_css else 0


if __name__ == "__main__":
    raise SystemExit(main())

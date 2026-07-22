#!/usr/bin/env python3
"""Captura y verifica el contrato no funcional de la homologación NAVI."""

from __future__ import annotations

import argparse
import hashlib
import json
import re
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
BASELINE = ROOT / "docs/parametrizacion/NAVI_HOMOLOGACION_BASELINE_CONTRACT.json"
PROJECT_ROOTS = (
    ROOT / "src/Navi.ToolsAssets.Admin",
    ROOT / "src/Navi.ToolsAssets.MobilePwa",
)
EXCLUDED = {"bin", "obj", "backup", "backups", "export_codigo", "node_modules"}

PROTECTED = (
    "src/Navi.ToolsAssets.Admin/Components/Pages/Tools.razor",
    "src/Navi.ToolsAssets.Admin/Components/Pages/Modules/AssetAvailability.razor",
    "src/Navi.ToolsAssets.Admin/Components/Pages/Modules/AssetAssignment.razor",
    "src/Navi.ToolsAssets.Admin/Components/Pages/Modules/AssetAssignmentHistory.razor",
    "src/Navi.ToolsAssets.Admin/Components/Pages/ToolDetail.razor",
    "src/Navi.ToolsAssets.Admin/Components/Pages/Tools.razor.css",
    "src/Navi.ToolsAssets.Admin/Components/Pages/Modules/AssetAvailability.razor.css",
    "src/Navi.ToolsAssets.Admin/Components/Pages/Modules/AssetAssignment.razor.css",
    "src/Navi.ToolsAssets.Admin/Components/Pages/Modules/AssetAssignmentHistory.razor.css",
    "src/Navi.ToolsAssets.Admin/Components/Pages/ToolDetail.razor.css",
    "src/Navi.ToolsAssets.Admin/wwwroot/css/ntx/inventory-web-v23.css",
    "src/Navi.ToolsAssets.Admin/wwwroot/css/ntx/asset-availability-web-v24.css",
    "src/Navi.ToolsAssets.Admin/wwwroot/css/ntx/assignment-web-v24.css",
    "src/Navi.ToolsAssets.Admin/wwwroot/css/ntx/assignment-history-web-v25.css",
    "src/Navi.ToolsAssets.Admin/wwwroot/css/ntx/assignment-history-web-v26.css",
    "src/Navi.ToolsAssets.Admin/wwwroot/css/ntx/tool-detail-web-v26.css",
    "src/Navi.ToolsAssets.MobilePwa/Pages/MyTools.razor",
    "src/Navi.ToolsAssets.MobilePwa/Pages/MobileAssetAvailability.razor",
    "src/Navi.ToolsAssets.MobilePwa/Pages/MobileAssignment.razor",
    "src/Navi.ToolsAssets.MobilePwa/Pages/MobileHistory.razor",
    "src/Navi.ToolsAssets.MobilePwa/Pages/MobileToolDetail.razor",
    "src/Navi.ToolsAssets.MobilePwa/wwwroot/css/ntx/mobile-inventory-v23.css",
    "src/Navi.ToolsAssets.MobilePwa/wwwroot/css/ntx/mobile-availability-v24.css",
    "src/Navi.ToolsAssets.MobilePwa/wwwroot/css/ntx/mobile-assignment-v25.css",
    "src/Navi.ToolsAssets.MobilePwa/wwwroot/css/ntx/mobile-assignment-history-v25.css",
)

DIRECTIVE_RE = re.compile(r"^\s*@(page|inject|attribute|rendermode|using)\b.*$", re.M)
EVENT_RE = re.compile(
    r"\b(@(?:bind(?::[\w-]+)?|onclick(?::[\w-]+)?|onchange|oninput|onsubmit|onkeydown|onkeyup))\s*=\s*"
    r"(?:\"([^\"]*)\"|'([^']*)')",
    re.I | re.S,
)
LINK_RE = re.compile(r"\b(href|action)\s*=\s*(?:\"([^\"]*)\"|'([^']*)')", re.I | re.S)
PERMISSION_RE = re.compile(
    r"<(?:PermissionGuard|AuthorizeView)\b(?:[^>\"']|\"[^\"]*\"|'[^']*')*>|"
    r"\b(?:Permission|Permissions|Policy|Roles)\s*=\s*(?:\"[^\"]*\"|'[^']*')",
    re.I | re.S,
)
CONTROL_FLOW_RE = re.compile(r"^\s*@(if|else\s+if|else|foreach|for|switch|while)\b.*$", re.M)
SERVICE_CALL_RE = re.compile(
    r"\b(?:Api|Client|Http|HttpClient|Auth|Navigation|NavigationManager|JS)\.\w+(?:Async)?\s*\(",
    re.I,
)


def digest(value: bytes | str) -> str:
    data = value.encode("utf-8") if isinstance(value, str) else value
    return hashlib.sha256(data).hexdigest()


def normalize(items: list[str]) -> list[str]:
    return sorted(" ".join(item.split()) for item in items)


def razor_files() -> list[Path]:
    result: list[Path] = []
    for root in PROJECT_ROOTS:
        for path in root.rglob("*.razor"):
            if path.is_file() and not any(part in EXCLUDED for part in path.parts):
                result.append(path)
    return sorted(result)


def functional_projection(path: Path) -> dict[str, object]:
    text = path.read_text(encoding="utf-8-sig", errors="replace")
    code_at = text.find("@code")
    functions_at = text.find("@functions")
    starts = [index for index in (code_at, functions_at) if index >= 0]
    code = text[min(starts):] if starts else ""
    events = [f"{match.group(1)}={match.group(2) or match.group(3) or ''}" for match in EVENT_RE.finditer(text)]
    links = [f"{match.group(1)}={match.group(2) or match.group(3) or ''}" for match in LINK_RE.finditer(text)]
    permissions = [match.group(0) for match in PERMISSION_RE.finditer(text)]
    controls = [match.group(0) for match in CONTROL_FLOW_RE.finditer(text)]
    service_calls = [match.group(0) for match in SERVICE_CALL_RE.finditer(text)]
    directives = [match.group(0) for match in DIRECTIVE_RE.finditer(text)]
    categories = {
        "directives": normalize(directives),
        "events": normalize(events),
        "links": normalize(links),
        "permissions": normalize(permissions),
        "control_flow": normalize(controls),
        "service_calls": normalize(service_calls),
    }
    return {
        "code_hash": digest(code),
        "code_present": bool(code),
        "categories": categories,
        "category_hashes": {name: digest("\n".join(values)) for name, values in categories.items()},
        "category_counts": {name: len(values) for name, values in categories.items()},
    }


def capture() -> dict[str, object]:
    protected: dict[str, str] = {}
    missing: list[str] = []
    for relative in PROTECTED:
        path = ROOT / relative
        if not path.exists():
            missing.append(relative)
            continue
        protected[relative] = digest(path.read_bytes())
    functional = {
        path.relative_to(ROOT).as_posix(): functional_projection(path)
        for path in razor_files()
    }
    return {
        "protected_hashes": protected,
        "missing_protected": missing,
        "functional_contract": functional,
        "summary": {
            "protected_files": len(protected),
            "razor_files": len(functional),
        },
    }


def verify(baseline: dict[str, object], current: dict[str, object]) -> dict[str, object]:
    errors: list[dict[str, object]] = []
    before_hashes = baseline["protected_hashes"]
    after_hashes = current["protected_hashes"]
    for path, expected in before_hashes.items():
        actual = after_hashes.get(path)
        if actual != expected:
            errors.append({"category": "protected_hash", "file": path, "before": expected, "after": actual})

    before_contract = baseline["functional_contract"]
    after_contract = current["functional_contract"]
    for path, expected in before_contract.items():
        actual = after_contract.get(path)
        if actual is None:
            errors.append({"category": "missing_razor", "file": path})
            continue
        if actual["code_hash"] != expected["code_hash"]:
            errors.append({"category": "code_hash", "file": path})
        for category, expected_hash in expected["category_hashes"].items():
            actual_hash = actual["category_hashes"].get(category)
            if actual_hash != expected_hash:
                errors.append(
                    {
                        "category": category,
                        "file": path,
                        "before_count": expected["category_counts"][category],
                        "after_count": actual["category_counts"].get(category),
                    }
                )
    added = sorted(set(after_contract) - set(before_contract))
    return {
        "passed": not errors,
        "errors": errors,
        "added_razor_files": added,
        "summary": {
            "protected_verified": len(before_hashes),
            "razor_verified": len(before_contract),
            "differences": len(errors),
        },
    }


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("mode", choices=("capture", "verify"))
    args = parser.parse_args()
    current = capture()
    if args.mode == "capture":
        BASELINE.parent.mkdir(parents=True, exist_ok=True)
        BASELINE.write_text(json.dumps(current, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
        print(json.dumps({"baseline": BASELINE.relative_to(ROOT).as_posix(), **current["summary"], "missing": current["missing_protected"]}, ensure_ascii=False, indent=2))
        return 1 if current["missing_protected"] else 0
    if not BASELINE.exists():
        raise SystemExit(f"No existe la línea base: {BASELINE}")
    baseline = json.loads(BASELINE.read_text(encoding="utf-8"))
    result = verify(baseline, current)
    print(json.dumps(result, ensure_ascii=False, indent=2))
    return 0 if result["passed"] else 1


if __name__ == "__main__":
    raise SystemExit(main())

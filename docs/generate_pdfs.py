#!/usr/bin/env python3
"""Markdown kaynaklarından docs/*.pdf üretir."""

from __future__ import annotations

import re
import sys
from pathlib import Path

from fpdf import FPDF

ROOT = Path(__file__).resolve().parent
SOURCE = ROOT / "source"
FONT = Path("/System/Library/Fonts/Supplemental/Arial Unicode.ttf")

OUTPUTS = {
    "SWOT.md": "SWOT.pdf",
    "RAMS.md": "RAMS.pdf",
    "THS_report.md": "THS_report.pdf",
    "Requirements.md": "Requirements.pdf",
    "UserScenario.md": "UserScenario.pdf",
}


class DocPDF(FPDF):
    def __init__(self, title: str):
        super().__init__()
        self.doc_title = title
        self.add_font("Body", "", str(FONT))
        self.add_font("Body", "B", str(FONT))
        self.set_margins(18, 18, 18)
        self.set_auto_page_break(auto=True, margin=18)

    def write_block(self, text: str, size: int = 10, bold: bool = False, line_h: float = 6) -> None:
        self.set_font("Body", "B" if bold else "", size)
        self.set_x(self.l_margin)
        self.multi_cell(self.epw, line_h, text)

    def header(self):
        self.set_font("Body", "B", 9)
        self.set_text_color(90, 90, 90)
        self.set_x(self.l_margin)
        self.cell(self.epw, 8, self.doc_title, align="R", new_x="LMARGIN", new_y="NEXT")
        self.ln(1)

    def footer(self):
        self.set_y(-14)
        self.set_font("Body", "", 8)
        self.set_text_color(120, 120, 120)
        self.cell(0, 8, f"Sayfa {self.page_no()}", align="C")


def strip_md_inline(text: str) -> str:
    text = text.replace("✅", "[OK]").replace("⚠️", "[!]").replace("❌", "[X]")
    text = re.sub(r"\*\*(.+?)\*\*", r"\1", text)
    text = re.sub(r"`(.+?)`", r"\1", text)
    text = re.sub(r"\[(.+?)\]\(.+?\)", r"\1", text)
    return text.strip()


def render_markdown(pdf: DocPDF, content: str) -> None:
    pdf.add_page()
    pdf.set_text_color(20, 20, 20)

    for raw_line in content.splitlines():
        line = raw_line.rstrip()
        stripped = line.strip()

        if not stripped:
            pdf.ln(3)
            continue

        if stripped == "---":
            pdf.ln(2)
            continue

        if stripped.startswith("# "):
            pdf.ln(4)
            pdf.write_block(strip_md_inline(stripped[2:]), size=16, bold=True, line_h=9)
            pdf.ln(2)
            continue

        if stripped.startswith("## "):
            pdf.ln(3)
            pdf.write_block(strip_md_inline(stripped[3:]), size=13, bold=True, line_h=8)
            pdf.ln(1)
            continue

        if stripped.startswith("### "):
            pdf.ln(2)
            pdf.write_block(strip_md_inline(stripped[4:]), size=11, bold=True, line_h=7)
            pdf.ln(1)
            continue

        if stripped.startswith("```"):
            continue

        if stripped.startswith("|") and stripped.endswith("|"):
            cells = [strip_md_inline(c) for c in stripped.strip("|").split("|")]
            if all(set(c) <= {"-", ":", " "} for c in cells):
                continue
            row = " | ".join(cells)
            pdf.write_block(row, size=9)
            continue

        if stripped.startswith("- [ ] "):
            pdf.write_block("[ ] " + strip_md_inline(stripped[6:]))
            continue

        if stripped.startswith("- [x] ") or stripped.startswith("- [X] "):
            pdf.write_block("[x] " + strip_md_inline(stripped[6:]))
            continue

        if stripped.startswith("- "):
            pdf.write_block("- " + strip_md_inline(stripped[2:]))
            continue

        if re.match(r"^\d+\.\s", stripped):
            pdf.write_block(strip_md_inline(stripped))
            continue

        pdf.write_block(strip_md_inline(stripped))


def build_pdf(src_name: str, out_name: str) -> None:
    src_path = SOURCE / src_name
    if not src_path.exists():
        raise FileNotFoundError(src_path)

    content = src_path.read_text(encoding="utf-8")
    title = strip_md_inline(content.splitlines()[0].lstrip("# ").strip())
    pdf = DocPDF(title)
    render_markdown(pdf, content)
    out_path = ROOT / out_name
    pdf.output(str(out_path))
    print(f"OK  {out_path.name}")


def main() -> int:
    if not FONT.exists():
        print(f"Font bulunamadı: {FONT}", file=sys.stderr)
        return 1

    SOURCE.mkdir(parents=True, exist_ok=True)

    for src, out in OUTPUTS.items():
        build_pdf(src, out)

    return 0


if __name__ == "__main__":
    raise SystemExit(main())

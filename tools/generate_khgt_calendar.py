#!/usr/bin/env python3
"""
Regenerates the KHGT (Kalender Hijriah Global Tunggal / Muhammadiyah) month-start table embedded in
IndonesianCalendar.cs, between the "BEGIN/END KHGT-GENERATED" markers.

khgt.muhammadiyah.or.id has no JSON/API — only a per-Hijri-year calendar PDF
(calendar-download?year={year}&mode=hijriah), one page per month, each page a day-grid table whose
cells look like "16 Jun\n١\nWage" (Gregorian date, Hijri day number in Arabic-Indic digits,
Javanese day name). This script downloads those PDFs and parses the grid with pdfplumber (day 1 of
each month is the cell whose second line is exactly "١") rather than trying to compute KHGT's
hisab (moon position / global visibility criteria) itself.

Deterministic-space tooling, not part of the running app — meant to be re-run periodically (see
.github/workflows/update-khgt-calendar.yml, which opens a PR rather than auto-merging: this is
worship-relevant calendar data and should get a human's eyes before it ships) to keep the table's
coverage comfortably ahead of "today".
"""
from __future__ import annotations

import re
import sys
import urllib.error
import urllib.request
from dataclasses import dataclass
from datetime import date, timedelta
from io import BytesIO
from pathlib import Path

import pdfplumber

PDF_URL = "https://khgt.muhammadiyah.or.id/calendar-download?year={year}&mode=hijriah"
START_HIJRI_YEAR = 1447  # First year already covered; the table only ever grows forward from here.
COVERAGE_TARGET_DAYS_AHEAD = 500  # Keep fetching further Hijri years until coverage reaches this far past "today".
MAX_YEARS_TO_FETCH = 12  # Safety cap so a bug can't spin this into an unbounded fetch loop.

CALENDAR_FILE = (
    Path(__file__).resolve().parent.parent / "src" / "JadwalSholat.Core" / "Services" / "IndonesianCalendar.cs"
)
BEGIN_MARKER = "    // BEGIN KHGT-GENERATED (tools/generate_khgt_calendar.py — jangan edit manual)"
END_MARKER = "    // END KHGT-GENERATED"

ARABIC_DIGIT_1 = "١"

ID_MONTHS_LONG = [
    "Januari", "Februari", "Maret", "April", "Mei", "Juni",
    "Juli", "Agustus", "September", "Oktober", "November", "Desember",
]
ID_MONTHS_ABBR = ["Jan", "Feb", "Mar", "Apr", "Mei", "Jun", "Jul", "Agt", "Sep", "Okt", "Nov", "Des"]

HEADER_RE = re.compile(r"^(\w+) (\d+) \((\w+) (\d{4}) - (\w+) (\d{4})\)$")
DAY_CELL_RE = re.compile(r"^\d{1,2} [A-Za-z]+$")


@dataclass
class MonthStart:
    start: date
    month: int  # 1-12
    hijri_year: int
    day_count: int


def fetch_pdf(hijri_year: int) -> bytes:
    url = PDF_URL.format(year=hijri_year)
    req = urllib.request.Request(url, headers={"User-Agent": "Mozilla/5.0 (JadwalSholat calendar sync)"})
    with urllib.request.urlopen(req, timeout=30) as resp:
        return resp.read()


def parse_year(hijri_year: int, pdf_bytes: bytes) -> list[MonthStart]:
    results: list[MonthStart] = []
    with pdfplumber.open(BytesIO(pdf_bytes)) as pdf:
        if len(pdf.pages) != 12:
            raise ValueError(f"{hijri_year}H: expected 12 pages (one per month), got {len(pdf.pages)}")

        for i, page in enumerate(pdf.pages):
            month_num = i + 1
            lines = (page.extract_text() or "").split("\n")
            if not lines:
                raise ValueError(f"{hijri_year}H month {month_num}: empty page")
            header_line = lines[1] if lines[0].startswith("Kalender Hijriah Global Tunggal") else lines[0]

            m = HEADER_RE.match(header_line)
            if not m:
                raise ValueError(f"{hijri_year}H month {month_num}: header mismatch: {header_line!r}")
            _, page_hijri_year, mon1, y1, mon2, y2 = m.groups()
            if int(page_hijri_year) != hijri_year:
                raise ValueError(f"{hijri_year}H month {month_num}: page reports year {page_hijri_year}")
            idx1, idx2 = ID_MONTHS_LONG.index(mon1), ID_MONTHS_LONG.index(mon2)

            tables = page.extract_tables()
            if len(tables) != 1:
                raise ValueError(f"{hijri_year}H month {month_num}: expected 1 table, got {len(tables)}")

            day_count = 0
            start_date_str: str | None = None
            for row in tables[0]:
                for cell in row:
                    if not cell:
                        continue
                    cell_lines = [ln.strip() for ln in cell.split("\n")]
                    first_line = cell_lines[0]
                    if not DAY_CELL_RE.match(first_line):
                        continue
                    day_count += 1
                    if len(cell_lines) > 1 and cell_lines[1] == ARABIC_DIGIT_1:
                        start_date_str = first_line

            if start_date_str is None:
                raise ValueError(f"{hijri_year}H month {month_num}: day-1 cell not found")

            day_str, mon_abbr = start_date_str.split()
            abbr_idx = ID_MONTHS_ABBR.index(mon_abbr)
            if abbr_idx == idx1:
                greg_year = int(y1)
            elif abbr_idx == idx2:
                greg_year = int(y2)
            else:
                raise ValueError(f"{hijri_year}H month {month_num}: month {mon_abbr} not in header range")

            if day_count not in (29, 30):
                raise ValueError(f"{hijri_year}H month {month_num}: implausible day count {day_count}")

            results.append(MonthStart(date(greg_year, abbr_idx + 1, int(day_str)), month_num, hijri_year, day_count))
    return results


def fetch_all(today: date) -> list[MonthStart]:
    months: list[MonthStart] = []
    year = START_HIJRI_YEAR
    for _ in range(MAX_YEARS_TO_FETCH):
        print(f"Fetching KHGT {year}H...", file=sys.stderr)
        try:
            pdf_bytes = fetch_pdf(year)
        except urllib.error.URLError as e:
            raise SystemExit(f"Failed to fetch {year}H: {e}") from e

        year_months = parse_year(year, pdf_bytes)
        months.extend(year_months)

        last = year_months[-1]
        coverage_end = last.start + timedelta(days=last.day_count - 1)
        print(f"  {year}H parsed OK, coverage now reaches {coverage_end}", file=sys.stderr)
        if coverage_end >= today + timedelta(days=COVERAGE_TARGET_DAYS_AHEAD):
            break
        year += 1
    else:
        raise SystemExit(f"Hit MAX_YEARS_TO_FETCH={MAX_YEARS_TO_FETCH} without reaching the coverage target")

    validate_contiguous(months)
    return months


def validate_contiguous(months: list[MonthStart]) -> None:
    for prev, cur in zip(months, months[1:]):
        expected_start = prev.start + timedelta(days=prev.day_count)
        if cur.start != expected_start:
            raise ValueError(
                f"gap/overlap between {prev.hijri_year}H month {prev.month} and "
                f"{cur.hijri_year}H month {cur.month}: expected {expected_start}, got {cur.start}"
            )


def render_csharp_block(months: list[MonthStart]) -> str:
    range_end = months[-1].start + timedelta(days=months[-1].day_count - 1)
    entries = "\n".join(
        f"        (new DateOnly({m.start.year}, {m.start.month}, {m.start.day}), {m.month}, {m.hijri_year}),"
        for m in months
    )
    return (
        f"{BEGIN_MARKER}\n"
        "    private static readonly (DateOnly Start, int Month, int Year)[] KhgtMonthStarts =\n"
        "    [\n"
        f"{entries}\n"
        "    ];\n"
        "\n"
        f"    private static readonly DateOnly KhgtRangeEnd = new({range_end.year}, {range_end.month}, {range_end.day});\n"
        f"{END_MARKER}"
    )


def replace_generated_block(new_block: str) -> bool:
    original = CALENDAR_FILE.read_text()
    pattern = re.compile(re.escape(BEGIN_MARKER) + r".*?" + re.escape(END_MARKER), re.DOTALL)
    if not pattern.search(original):
        raise SystemExit(f"Could not find {BEGIN_MARKER!r} ... {END_MARKER!r} block in {CALENDAR_FILE}")

    updated = pattern.sub(new_block, original)
    if updated == original:
        return False
    CALENDAR_FILE.write_text(updated)
    return True


def main() -> None:
    today = date.today()
    months = fetch_all(today)
    block = render_csharp_block(months)
    changed = replace_generated_block(block)

    span = f"{months[0].start} .. {months[-1].start + timedelta(days=months[-1].day_count - 1)}"
    if changed:
        print(f"Updated {CALENDAR_FILE} — KHGT coverage now {span} ({len(months)} months).")
    else:
        print(f"No change — KHGT coverage already {span} ({len(months)} months).")


if __name__ == "__main__":
    main()

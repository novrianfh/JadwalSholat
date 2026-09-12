#!/usr/bin/env python3
"""
Fetches a rolling 12-month prayer time dataset for the 5 bundled cities from api.myquran.com
and writes it as static JSON consumed by JadwalSholat.Web (wwwroot/data/prayertimes/{cityId}.json).

This is deterministic-space tooling, not part of the running app: it is meant to be re-run
periodically (e.g. yearly, via cron or by hand) to refresh the bundled dataset, so the Blazor
app never has to call the API at runtime (Requirement.md: "kalau bisa, ambil jadwal sholat ...
selama 1 tahun agar tidak perlu feed tiap hari/bulan").

The JSON schema this writes is the contract also modeled in C# by
src/JadwalSholat.Core/Models/CityScheduleFile.cs — keep both in sync if it changes.
"""
from __future__ import annotations

import json
import sys
import time
import urllib.error
import urllib.request
from dataclasses import dataclass
from datetime import date, datetime, timezone
from pathlib import Path

API_BASE = "https://api.myquran.com/v2/sholat/jadwal"
MONTHS_AHEAD = 12
REQUEST_DELAY_SECONDS = 0.3
MAX_RETRIES = 3

OUTPUT_DIR = Path(__file__).resolve().parent.parent / "src" / "JadwalSholat.Web" / "wwwroot" / "data" / "prayertimes"

# Must match JadwalSholat.Core.Models.City.Bundled exactly (id + display name).
CITIES = [
    ("1301", "Jakarta"),
    ("1219", "Bandung"),
    ("1433", "Semarang"),
    ("1505", "Yogyakarta"),
    ("1638", "Surabaya"),
]

# API field -> our schema field. "imsak" and "dhuha" are dropped: Requirement.md asks for
# exactly the 5 obligatory prayers plus Shuruk (sunrise), nothing more.
FIELD_MAP = {
    "subuh": "fajr",
    "terbit": "shuruk",
    "dzuhur": "dhuhr",
    "ashar": "asr",
    "maghrib": "maghrib",
    "isya": "isha",
}


@dataclass
class MonthKey:
    year: int
    month: int

    def next(self) -> "MonthKey":
        return MonthKey(self.year + 1, 1) if self.month == 12 else MonthKey(self.year, self.month + 1)


def fetch_month(city_id: str, ym: MonthKey) -> list[dict]:
    url = f"{API_BASE}/{city_id}/{ym.year}/{ym.month}"
    last_error: Exception | None = None
    for attempt in range(1, MAX_RETRIES + 1):
        try:
            request = urllib.request.Request(url, headers={"User-Agent": "curl/8.7.1"})
            with urllib.request.urlopen(request, timeout=15) as resp:
                payload = json.loads(resp.read().decode("utf-8"))
            if not payload.get("status"):
                raise ValueError(f"API returned status=false for {url}")
            return payload["data"]["jadwal"]
        except (urllib.error.URLError, ValueError, KeyError) as exc:
            last_error = exc
            print(f"  retry {attempt}/{MAX_RETRIES} for {url}: {exc}", file=sys.stderr)
            time.sleep(1.0 * attempt)
    raise RuntimeError(f"Failed to fetch {url} after {MAX_RETRIES} attempts") from last_error


def normalize_day(raw: dict) -> dict:
    day = {"date": raw["date"]}
    for api_field, our_field in FIELD_MAP.items():
        value = raw[api_field]
        if len(value) != 5 or value[2] != ":":
            raise ValueError(f"Unexpected time format {value!r} for field {api_field} on {raw['date']}")
        day[our_field] = value
    return day


def generate_city(city_id: str, city_name: str, start: MonthKey) -> dict:
    days: dict[str, dict] = {}
    ym = start
    for _ in range(MONTHS_AHEAD):
        print(f"  fetching {city_name} {ym.year}-{ym.month:02d}...")
        for raw_day in fetch_month(city_id, ym):
            day = normalize_day(raw_day)
            days[day["date"]] = day
        time.sleep(REQUEST_DELAY_SECONDS)
        ym = ym.next()

    ordered_days = [days[k] for k in sorted(days.keys())]
    return {
        "cityId": city_id,
        "cityName": city_name,
        "generatedAtUtc": datetime.now(timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ"),
        "source": "api.myquran.com",
        "days": ordered_days,
    }


def validate(city_data: dict, expected_start: date) -> None:
    days = city_data["days"]
    if not days:
        raise ValueError(f"{city_data['cityName']}: no days generated")

    parsed_dates = [datetime.strptime(d["date"], "%Y-%m-%d").date() for d in days]
    if parsed_dates != sorted(parsed_dates):
        raise ValueError(f"{city_data['cityName']}: dates are not sorted")
    if len(set(parsed_dates)) != len(parsed_dates):
        raise ValueError(f"{city_data['cityName']}: duplicate dates found")
    if parsed_dates[0] != expected_start:
        raise ValueError(f"{city_data['cityName']}: first date {parsed_dates[0]} != expected {expected_start}")

    span_days = (parsed_dates[-1] - parsed_dates[0]).days + 1
    if span_days < 300:
        raise ValueError(f"{city_data['cityName']}: only {span_days} days of coverage, expected ~365")

    for d in days:
        for field in ("fajr", "shuruk", "dhuhr", "asr", "maghrib", "isha"):
            hh, mm = d[field].split(":")
            if not (0 <= int(hh) <= 23 and 0 <= int(mm) <= 59):
                raise ValueError(f"{city_data['cityName']} {d['date']}: invalid time {field}={d[field]}")


def main() -> None:
    today = date.today()
    start = MonthKey(today.year, today.month)
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)

    for city_id, city_name in CITIES:
        print(f"Generating {city_name} ({city_id})...")
        city_data = generate_city(city_id, city_name, start)
        validate(city_data, expected_start=date(today.year, today.month, 1))

        out_path = OUTPUT_DIR / f"{city_id}.json"
        out_path.write_text(json.dumps(city_data, ensure_ascii=False, indent=None, separators=(",", ":")))
        print(f"  wrote {out_path} ({len(city_data['days'])} days, {out_path.stat().st_size:,} bytes)")

    print("Done.")


if __name__ == "__main__":
    main()

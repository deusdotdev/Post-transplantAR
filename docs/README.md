# Proje Dokümantasyonu — Post-transplantAR

Hoca teslimi için akademik belgeler. PDF'ler bu klasörde; kaynak Markdown dosyaları `source/` altındadır.

## Dosyalar

| Belge | PDF | Puan | Açıklama |
|-------|-----|------|----------|
| SWOT analizi | [SWOT.pdf](SWOT.pdf) | 10 | Güçlü/zayıf yönler, fırsatlar, tehditler |
| RAMS raporu | [RAMS.pdf](RAMS.pdf) | 5 | Reliability, Availability, Maintainability, Safety |
| THS raporu | [THS_report.pdf](THS_report.pdf) | 5 | Teknoloji Hazırlık Seviyesi — kullanıma hazırlık |
| Gereksinim analizi | [Requirements.pdf](Requirements.pdf) | 5 | İşlevsel / işlevsel olmayan gereksinimler |
| Kullanım senaryoları | [UserScenario.pdf](UserScenario.pdf) | 5 | Persona ve kullanım akışları (UC-01 … UC-05) |

## PDF yeniden üretme

```bash
python3 -m venv .venv
.venv/bin/pip install fpdf2
.venv/bin/python docs/generate_pdfs.py
```

`source/` içindeki `.md` dosyalarını düzenledikten sonra yukarıdaki komutu çalıştırın.

## Kaynak dosyalar

- `source/SWOT.md`
- `source/RAMS.md`
- `source/THS_report.md`
- `source/Requirements.md`
- `source/UserScenario.md`

Kök dizindeki `swot.md`, `RAMS.md` ve `GEREKSINIM_ANALIZI.md` dosyaları geliştirme sırasında kullanılan canlı kopyalardır; teslim için `docs/` altındaki PDF'ler esas alınır.

# Samples

Ready to run pipelines for the `ColourClashNetCli` command line tool and for the **Pipeline** menu of the application.
Every pipeline writes its results in an `output` folder next to it (ignored by git).

```bash
ColourClashNetCli run Samples/Pipelines/Transformations/zx-autotune.json
```
```bash
ColourClashNetCli run Samples/Pipelines/Transformations/zx-autotune.json --input photo.png --output photo-zx.png
```
```bash
ColourClashNetCli run Samples
```
```bash
ColourClashNetCli validate Samples
```

The continuous integration validates and runs all of them on every push; the results can be downloaded
from the run page (artifact `sample-pipeline-results`).

## Images
| File | Content |
|-|-|
| `Images/sprite-sheet.png` | 10 sprites on a magenta background, plus two 1-2 pixel specks (removed by the size filters) |
| `../Images/Original.png` | Photo used by the transformation, region and combo samples |

## Pipelines

### Transformations
| File | Result |
|-|-|
| `clustering-16.json` | K-means, 16 colors, Floyd-Steinberg |
| `median-cut-8.json` | Median cut, 8 colors, ordered 4x4 |
| `atari-st-rgb333.json` | Atari ST: RGB333 color space, 16 colors, 320x200 |
| `ega.json` | IBM EGA 16 color palette |
| `cga-composite.json` | IBM CGA composite artifact colors |
| `vga-256.json` | IBM VGA: RGB666, 256 colors |
| `cpc-mode0.json` | Amstrad CPC mode 0, wide pixels |
| `c64-hires.json` | C64 hires bitmap, 2 colors per 8x8 cell |
| `c64-multicolor.json` | C64 multicolor bitmap, Floyd-Steinberg |
| `zx-autotune.json` | ZX Spectrum attributes, fast seed search |
| `amiga-ham6.json` | Amiga HAM6 with fringe reduction |
| `amiga-ehb.json` | Amiga Extra Half-Brite |
| `amiga-copper-scanline.json` | Amiga OCS, 16 colors per group of 4 lines (copper) |
| `gameboy-palette.json` | Fixed 4 color palette (Game Boy greens) |
| `hsv-boost.json` | Hue rotation and saturation boost |

### Blob processing
| File | Result |
|-|-|
| `extract-sprites.json` | Every sprite saved as a PNG in `output/sprites`, plus all sprites stacked |
| `big-sprites-only.json` | Only the sprites of at least 24x24 pixels (size filters) |

### Regions of interest
| File | Result |
|-|-|
| `rectangle.json` | Rectangular region (every pixel kept) |
| `ellipse-portrait.json` | Elliptic region, black outside |
| `polygon.json` | Pentagon, magenta outside (ready for a background removal) |

### Combos
| File | Result |
|-|-|
| `multi-platform.json` | One photo converted for ZX Spectrum, C64, Amstrad CPC and Amiga in a single run |
| `portrait-cpc.json` | Ellipse portrait, background replaced with black, then CPC mode 0 |
| `sprites-gameboy.json` | Sprites extracted from the sheet, then mapped to the Game Boy palette |
| `retro-poster.json` | Saturation boost, RGB222, 8 colors with ordered dithering, 4x pixels |

## Writing a pipeline
- Start from a template: `ColourClashNetCli template C64 > my-pipeline.json` (`ColourClashNetCli list` shows the names).
- Steps: `Load`, `Crop`, `Resize`, `Roi`, `Transform`, `Blob`, `Save`; see the main README for their settings.
- Colors are written as `"#RRGGBB"`; comments (`//`) and trailing commas are allowed.
- Relative paths are relative to the folder of the pipeline file.

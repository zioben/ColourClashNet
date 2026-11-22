# ColorTransformBase simplification (planned, not started)

Status: **on hold**, to be done later on request.
Goal: reduce `ColourClashLib/Color/Transformation/ColorTransformBase.cs` (about 560 lines) to about 300,
without changing the algorithms.

## Findings (usage counted in the code)
- 4 ways to process an image: `Create` + `ProcessColors`, `CreateAndProcessColors`, 2 `Async` variants
  (the async ones only wrap `Task.Run`).
- 2 hooks for derived classes: `CreateTransformationMap` (overridden by 6 classes) and `ExecuteTransform` (10 classes);
  the default `CreateTransformationMap` does nothing but is always called.
- `BypassDithering` is mutable state set to `true` inside `ExecuteTransform` by 8 transformations.
- 6 events: `Creating`/`Created` have no subscribers, `ProcessAdvance` is only forwarded and never raised;
  `ProcessPartialEventRegistered` has 1 use; `AbortProcessing`/`AbortProcessingAsync` only call `Cancel()`.
- `TransformationError` has a public setter and is assigned in 12 places; the processing time lives in a stopwatch field.
- `Type` is set by hand in every constructor although the options already know `TransformType`
  (the Amiga `Type` bug came from this duplication).
- `ProcessColors` is a single method of about 140 lines with nested `else if`.

## Proposal (by value)
1. **Single entry point**: `ColorTransformResult Process(ImageData source, ImageData? reference = null, CancellationToken token = default)`.
   Remove the two step `Create`/`ProcessColors` form and the `Async` wrappers; `ImageSource`, `ImageReference`
   and `ImageOutput` stay available to derived classes during the run only.
2. **Single hook**: `protected abstract ColorTransformResult Transform(CancellationToken)` in the base class, plus a
   `ColorTransformMapBase` subclass with `BuildMap()` (build map + apply it) for Fast, MedianCut, Clustering,
   Quantization and the palette mappings. Platform transformations implement `Transform` directly.
3. **Dithering as a class property**: `protected virtual bool HandlesDitheringItself => false;`
   (CGA: `IsCompositeMode(Options.VideoMode)`) instead of `BypassDithering`.
4. **Fewer events**: keep `Processing`, `Progress` (replaces `ProcessPartial`) and `Processed`;
   drop `Creating`, `Created`, `ProcessAdvance`, `ProcessPartialEventRegistered`, `AbortProcessing(Async)`.
5. **Metrics in the result**: transformation error and processing time in `ColorTransformResult`, computed once by
   the base class; ZX returns its merge error in the result.
6. **`Type => Options.TransformType`** instead of setting it in the constructors.
7. **Split `ProcessColors`** into short private steps: Validate, Transform, Dither (when needed), Evaluate, Processed.

## Keep as is
- Options model with `ColorTransformBase<TOptions>`.
- `ForwardEventsFrom` and `LogProcessingSteps`.
- Error handling with `ColorTransformResult`.

## Impact and plan
Callers to adapt: `ColorTransformManager`, `TileProcessing`, `ProcessingForm`, `ProcessingViewer`, `FormTester`,
about 40 test calls (mostly `CreateAndProcessColors(image)`).

Two steps, each with green build and tests:
1. low risk: points 3, 4, 6, 7;
2. API shape change: points 1, 2, 5.

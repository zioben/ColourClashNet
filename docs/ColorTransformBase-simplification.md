# ColorTransformBase simplification

Status: **done**. This page records what was changed and why.

## Before
- 4 ways to process an image: `Create` + `ProcessColors`, `CreateAndProcessColors`, 2 `Async` variants.
- 2 hooks: `CreateTransformationMap` and `ExecuteTransform`, the first called (empty) even when unused.
- 6 events, 2 never used (`Creating`, `Created`) and 1 never raised (`ProcessAdvance`);
  `Processed` was not raised when the processing threw.
- `Type` set by hand in every constructor (the Amiga `Type` bug came from this duplication).
- `TransformationError` with a public setter, assigned in many places.
- `ProcessColors` was a single method of about 140 lines; the cancellation token published in the
  events was not the one used by the processing steps, so cancelling it did not stop the run.

## After
1. **Single entry point**: `Process(source, reference = null, token)` and `ProcessAsync(...)`.
   Errors and cancellation are returned in the result, never thrown.
2. **Single hook**: `protected abstract ExecuteTransform(token)`. Map based transformations derive from
   `ColorTransformMapBase<TOptions>` and only implement `CreateTransformationMap`
   (Fast, MedianCut, Clustering, Quantization, palette mappings, background removal).
3. **`BypassDithering` kept**, with a protected setter: a transformation sets it inside `ExecuteTransform`
   when it dithers by itself or when dithering does not apply in its current mode. It is reset at every run.
4. **Events**: `StartProcessing` (can change the options or cancel), `Creating`/`Created` (image loading),
   `Processing`, `Progress` (replaces `ProcessPartial`/`ProcessAdvance`), `Dithering`, `Completed`
   (always raised, also on error or cancellation), `Destroying`/`Destroyed` (raised by `Dispose`).
   A failing event handler is logged and never breaks the run; the published token source stops the run.
5. **Metrics in the result**: `ColorTransformResult.TransformationError` and `ProcessingTimeMilliseconds`,
   computed once by the base class (also kept on the transformation for the last run).
6. **`Type => Options.TransformType`**: the constructors no longer set it.
7. **Short steps**: `LoadImages`, `ValidateOptions`, `Transform`, `ApplyDithering`.

The interface is now `IColorTransform` (it extends `IDisposable`); the factory is `ColorTransformBase.CreateTransform`.

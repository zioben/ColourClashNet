using ColourClashNet.Color;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Log;
using ColourClashNet.Pipeline;
using System.Reflection;
using System.Text.Json;

namespace ColourClash
{
    /// <summary>
    /// Command line front-end: runs, validates and creates JSON processing pipelines.
    /// <para/>
    /// Exit codes: 0 success, 1 the pipeline failed or is invalid, 2 wrong command line or unreadable file.
    /// </summary>
    internal static class Program
    {
        const int ExitOk = 0;
        const int ExitFailed = 1;
        const int ExitUsage = 2;

        static int Main(string[] args)
        {
            if (args.Length == 0 || args[0] is "help" or "--help" or "-h" or "/?")
            {
                Help();
                return args.Length == 0 ? ExitUsage : ExitOk;
            }
            try
            {
                var command = args[0].ToLowerInvariant();
                var arguments = CommandLine.Parse(args.Skip(1).ToArray());
                if (arguments.Has("verbose"))
                    EnableConsoleLog();
                return command switch
                {
                    "run" => Run(arguments),
                    "validate" => Validate(arguments),
                    "template" => Template(arguments),
                    "list" => List(),
                    _ => Usage($"unknown command '{args[0]}'"),
                };
            }
            catch (ArgumentException ex)
            {
                return Usage(ex.Message);
            }
        }

        /// <summary>
        /// <c>run pipeline.json [--input file] [--output file] [--verbose]</c>
        /// </summary>
        static int Run(CommandLine arguments)
        {
            var (pipeline, file) = LoadPipeline(arguments);
            if (pipeline == null)
                return ExitUsage;
            if (arguments.Value("input") is string input)
                pipeline = pipeline.WithInput(Path.GetFullPath(input));
            if (arguments.Value("output") is string output)
                pipeline = pipeline.WithOutput(Path.GetFullPath(output));

            var runner = new PipelineRunner { BaseDirectory = Path.GetDirectoryName(Path.GetFullPath(file!)) ?? string.Empty };
            runner.StepStarted += (s, e) => Console.WriteLine($"[{e.Number}/{e.Count}] {e.Step}");
            runner.StepCompleted += (s, e) => Console.WriteLine($"      {(e.Result!.IsSuccess ? "ok" : "FAILED")} : {e.Result.Message} ({e.Result.ProcessingTimeMilliseconds:f0} ms)");

            using var cts = new CancellationTokenSource();
            // Ctrl+C cancels the run cleanly instead of killing the process
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };
            var result = runner.Run(pipeline, null, cts.Token);
            Console.WriteLine(result.IsSuccess
                ? $"Completed in {result.ProcessingTimeMilliseconds:f0} ms"
                : $"FAILED : {result.Message}");
            return result.IsSuccess ? ExitOk : ExitFailed;
        }

        /// <summary>
        /// <c>validate pipeline.json</c>
        /// </summary>
        static int Validate(CommandLine arguments)
        {
            var (pipeline, file) = LoadPipeline(arguments);
            if (pipeline == null)
                return ExitUsage;
            var errors = pipeline.Validate().ToList();
            if (errors.Count == 0)
            {
                Console.WriteLine($"{file} : valid, version {pipeline.Version}, {pipeline.Steps.Count} steps");
                foreach (var step in pipeline.Steps)
                    Console.WriteLine($"  - {step}");
                return ExitOk;
            }
            Console.Error.WriteLine($"{file} : {errors.Count} problems");
            foreach (var error in errors)
                Console.Error.WriteLine($"  - {error}");
            return ExitFailed;
        }

        /// <summary>
        /// <c>template TransformationType [--input file] [--output file]</c>: prints a pipeline with the default options.
        /// </summary>
        static int Template(CommandLine arguments)
        {
            var typeName = arguments.Positional(0) ?? throw new ArgumentException("template needs a transformation type (see 'list')");
            if (!Enum.TryParse<ColorTransformType>(typeName, true, out var type) &&
                !Enum.TryParse($"ColorReduction{typeName}", true, out type))
                throw new ArgumentException($"unknown transformation type '{typeName}' (see 'list')");

            var pipeline = new PipelineDefinition
            {
                Name = $"{type} template",
                Description = "Created by 'ColourClash template': edit the steps and the options as needed",
                Steps = new PipelineStep[]
                {
                    new LoadStep { File = arguments.Value("input") ?? "input.png" },
                    new ResizeStep { Width = 320, Height = 0, Mode = ResizeMode.Average, Label = "optional: remove it to keep the size" },
                    new TransformStep { Options = TransformOptions.CreateDefault(type) },
                    new SaveStep { File = arguments.Value("output") ?? "output.png" },
                },
            };
            Console.WriteLine(PipelineSerializer.Serialize(pipeline));
            return ExitOk;
        }

        /// <summary>
        /// <c>list</c>: prints the transformation types usable in a template.
        /// </summary>
        static int List()
        {
            const string prefix = "ColorReduction";
            foreach (var type in Enum.GetValues<ColorTransformType>().Where(t => t != ColorTransformType.None))
            {
                var name = type.ToString();
                Console.WriteLine(name.StartsWith(prefix) ? $"{name[prefix.Length..],-16} ({name})" : name);
            }
            return ExitOk;
        }

        /// <summary>
        /// Loads the pipeline named by the first positional argument; prints the error when it cannot.
        /// </summary>
        static (PipelineDefinition? pipeline, string? file) LoadPipeline(CommandLine arguments)
        {
            var file = arguments.Positional(0) ?? throw new ArgumentException("missing pipeline file");
            try
            {
                return (PipelineSerializer.Load(file), file);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
            {
                Console.Error.WriteLine($"{file} : {ex.Message}");
                return (null, file);
            }
        }

        /// <summary>
        /// Prints the log messages (debug and above) on the console.
        /// </summary>
        static void EnableConsoleLog()
        {
            LogMan.MinLogLevel = LogLevel.Debug;
            LogMan.OnLogMessage += (s, e) => Console.Error.WriteLine(e.Message.TrimEnd());
        }

        /// <summary>
        /// Prints an error and the help.
        /// </summary>
        static int Usage(string error)
        {
            Console.Error.WriteLine($"Error : {error}");
            Console.Error.WriteLine();
            Help();
            return ExitUsage;
        }

        /// <summary>
        /// Prints the command line syntax.
        /// </summary>
        static void Help()
        {
            var name = Assembly.GetExecutingAssembly().GetName();
            Console.WriteLine($"{name.Name} {name.Version} - retro image conversion pipelines");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine($"  {name.Name} run <pipeline.json> [--input <image>] [--output <image>] [--verbose]");
            Console.WriteLine("      runs the pipeline; --input replaces the first load step, --output the last save step");
            Console.WriteLine($"  {name.Name} validate <pipeline.json>");
            Console.WriteLine("      checks the pipeline without running it");
            Console.WriteLine($"  {name.Name} template <transformation> [--input <image>] [--output <image>]");
            Console.WriteLine("      prints a pipeline with the default options of a transformation (redirect it to a .json file)");
            Console.WriteLine($"  {name.Name} list");
            Console.WriteLine("      lists the transformations");
            Console.WriteLine();
            Console.WriteLine("Relative paths in a pipeline are relative to the folder of the pipeline file.");
            Console.WriteLine("Exit codes: 0 success, 1 pipeline failed or invalid, 2 wrong command line or unreadable file.");
        }
    }

    /// <summary>
    /// Minimal command line parser: positional arguments plus <c>--name value</c> options and <c>--flag</c> switches.
    /// </summary>
    sealed class CommandLine
    {
        // Options that take a value; any other --name is a switch
        static readonly HashSet<string> valueOptions = new(StringComparer.OrdinalIgnoreCase) { "input", "output" };

        readonly List<string> positional = new();
        readonly Dictionary<string, string?> options = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Parses the arguments after the command name.
        /// </summary>
        /// <exception cref="ArgumentException">An option is missing its value.</exception>
        public static CommandLine Parse(string[] args)
        {
            var commandLine = new CommandLine();
            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                if (arg.StartsWith("--"))
                {
                    var name = arg[2..];
                    if (valueOptions.Contains(name))
                    {
                        if (i + 1 >= args.Length)
                            throw new ArgumentException($"option --{name} needs a value");
                        commandLine.options[name] = args[++i];
                    }
                    else
                    {
                        commandLine.options[name] = null;
                    }
                }
                else
                {
                    commandLine.positional.Add(arg);
                }
            }
            return commandLine;
        }

        /// <summary>Positional argument <paramref name="index"/>, or null.</summary>
        public string? Positional(int index) => index < positional.Count ? positional[index] : null;

        /// <summary>Value of the option <paramref name="name"/>, or null.</summary>
        public string? Value(string name) => options.TryGetValue(name, out var value) ? value : null;

        /// <summary>True when the switch or option <paramref name="name"/> is present.</summary>
        public bool Has(string name) => options.ContainsKey(name);
    }
}

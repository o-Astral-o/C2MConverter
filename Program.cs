using Serilog;

namespace C2MConverter;

public class Program
{
    static void Main(string[] args)
    {
        // Logger
        Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

        string filePath;
#if DEBUG
        filePath = @"mp_jup_security_area.c2m";
#else
            if(args.Length < 0)
            {
                Log.Error("No arguments provided. Please specify the path to the C2M map file.");
                return;
            }

            filePath = args[0];
            if(!File.Exists(filePath))
            {
                Log.Error($"File not found: {filePath}");
                return;
            }
#endif
        var c2m = new C2M(File.ReadAllBytes(filePath));
        var directory = Path.GetDirectoryName(filePath);

        c2m.SaveAsCast(directory);
    }
}
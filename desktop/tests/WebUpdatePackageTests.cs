using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;

internal static class WebUpdatePackageTests
{
    private const BindingFlags Hidden = BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
    private static object Call(Type type, object instance, string method, params object[] arguments)
    {
        return type.GetMethod(method, Hidden).Invoke(instance, arguments);
    }

    private static int Main(string[] args)
    {
        try
        {
            // All extraction targets are fresh test directories supplied by the caller.
            if (args.Length != 6) throw new ArgumentException("host.exe manifest assets base16.zip base17.zip output");
            string output = Path.GetFullPath(args[5]);
            if (Directory.Exists(output)) throw new IOException("Test output must be new");
            Directory.CreateDirectory(output);
            Assembly host = Assembly.LoadFrom(Path.GetFullPath(args[0]));
            Type windowType = host.GetType("MK3MoveBook.MoveBookWindow", true);
            object window = FormatterServices.GetUninitializedObject(windowType);
            windowType.GetField("jsonSerializer", Hidden).SetValue(window, new JavaScriptSerializer());
            object manifest = Call(windowType, window, "ParseManifest", File.ReadAllText(args[1]));
            foreach (string version in new[] { "1.1.16", "1.1.17" })
            {
                windowType.GetField("currentContentVersion", Hidden).SetValue(window, version);
                object selected = Call(windowType, window, "SelectUpdatePackage", manifest);
                Type selectedType = selected.GetType();
                if (!(bool)selectedType.GetProperty("IsDelta").GetValue(selected, null)) throw new Exception("Delta was not selected for " + version);
                string url = (string)selectedType.GetProperty("PackageUrl").GetValue(selected, null);
                string archive = Path.Combine(args[2], Path.GetFileName(new Uri(url).AbsolutePath));
                string expectedHash = (string)selectedType.GetProperty("Sha256").GetValue(selected, null);
                if (!string.Equals(expectedHash, (string)Call(windowType, null, "ComputeSha256", archive), StringComparison.OrdinalIgnoreCase)) throw new Exception("Downloaded archive checksum mismatch");
                string stage = Path.Combine(output, version);
                string baseArchive = version == "1.1.16" ? args[3] : args[4];
                Call(windowType, null, "ExtractZipSafely", baseArchive, stage);
                Call(windowType, null, "ExtractZipSafely", archive, stage);
                string web = Path.Combine(stage, "web");
                Call(windowType, null, "ApplyDeltaDeletedFiles", stage, web);
                Call(windowType, null, "VerifyDeltaTargetFiles", stage, web);
                if (!File.ReadAllText(Path.Combine(web, "version.json")).Contains("1.1.18")) throw new Exception("Target version mismatch");
                Console.WriteLine("PASS: original host selects, checks SHA256, extracts and verifies " + version + " -> 1.1.18");
                string portrait = Path.Combine(web, "assets", "portraits", "kabal.png");
                File.WriteAllText(portrait, "corrupt fixture");
                bool rejected = false;
                try { Call(windowType, null, "VerifyDeltaTargetFiles", stage, web); }
                catch (TargetInvocationException error) { rejected = error.InnerException is InvalidDataException; }
                if (!rejected) throw new Exception("Corrupt resource was accepted");
                // Restore the test installation through the same verified package.
                if (version == "1.1.16") Call(windowType, null, "ExtractZipSafely", baseArchive, stage);
                Call(windowType, null, "ExtractZipSafely", archive, stage);
                Call(windowType, null, "VerifyDeltaTargetFiles", stage, web);
                Console.WriteLine("PASS: corrupted portrait rejected and restored for " + version);
            }
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
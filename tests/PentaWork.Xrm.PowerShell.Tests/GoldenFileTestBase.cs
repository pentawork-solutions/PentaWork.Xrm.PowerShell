namespace PentaWork.Xrm.PowerShell.Tests
{
    public abstract class GoldenFileTestBase
    {
        protected static string GoldenFilesDirectory
        {
            get
            {
                var dir = new DirectoryInfo(AppContext.BaseDirectory);
                while (dir != null && !File.Exists(Path.Combine(dir.FullName, "PentaWork.Xrm.PowerShell.Tests.csproj")))
                    dir = dir.Parent;

                if (dir == null)
                    throw new InvalidOperationException("Could not locate the test project directory from " + AppContext.BaseDirectory);

                return Path.Combine(dir.FullName, "GoldenFiles");
            }
        }

        protected static void WriteGoldenFile(string name, string content)
        {
            Directory.CreateDirectory(GoldenFilesDirectory);
            File.WriteAllText(Path.Combine(GoldenFilesDirectory, name), content);
        }

        protected static string ReadGoldenFile(string name)
        {
            return File.ReadAllText(Path.Combine(GoldenFilesDirectory, name));
        }

        /// <summary>
        /// T4 and Scriban place newlines/whitespace around control blocks differently (e.g. a
        /// trailing ";" ending up on its own line), so exact byte or line comparison isn't
        /// meaningful here. Compare on whitespace-collapsed tokens instead - catches real
        /// data/logic/ordering regressions while ignoring cosmetic line-break/indentation drift.
        /// </summary>
        protected static void AssertMatchesGoldenFile(string goldenFileName, string actual)
        {
            var expectedTokens = NormalizeTokens(ReadGoldenFile(goldenFileName));
            var actualTokens = NormalizeTokens(actual);
            CollectionAssert.AreEqual(expectedTokens, actualTokens, $"Rendered output does not match {goldenFileName}");
        }

        private static List<string> NormalizeTokens(string text)
        {
            return text
                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
                .ToList();
        }
    }
}

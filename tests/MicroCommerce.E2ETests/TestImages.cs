namespace MicroCommerce.E2ETests;

/// <summary>
/// Two small, decodable PNGs (8×8, red and blue). Unlike the functional tests' signature-only bytes,
/// the browser has to render these.
/// </summary>
public static class TestImages
{
    public static byte[] Png { get; } = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAgAAAAICAIAAABLbSncAAAAEUlEQVR4nGO4o6GBFTEMLQkAe3tLAfuiUfAAAAAASUVORK5CYII=");

    public static byte[] PngAlt { get; } = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAgAAAAICAIAAABLbSncAAAAEUlEQVR4nGPQCLiDFTEMLQkACbdVAfTkR+4AAAAASUVORK5CYII=");

    /// <summary>Writes the bytes to a fresh temporary file, for <c>SetInputFilesAsync</c>.</summary>
    public static string WriteTempFile(byte[] bytes, string name)
    {
        var dir = Directory.CreateTempSubdirectory("e2e-images-").FullName;
        var path = Path.Combine(dir, name);
        File.WriteAllBytes(path, bytes);
        return path;
    }
}

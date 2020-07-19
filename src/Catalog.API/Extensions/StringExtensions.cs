using System;
using System.IO;

namespace Catalog.API.Extensions
{
    public static class StringExtensions
    {
        public static string ToFileName(this string fileName)
        {
            return $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}{Path.GetExtension(fileName)}";
        }
    }
}

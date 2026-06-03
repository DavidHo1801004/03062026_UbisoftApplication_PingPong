using System.Text.RegularExpressions;

namespace GASU.Utilities
{
    /// <summary>
    /// Custom string functions using <see cref="System.Text.RegularExpressions"/>.
    /// </summary>
    public static class RegExUtils
    {
        /// <summary>
        /// Parse the given text using camel case parsing.
        /// </summary>
        /// <param name="_Input"> Text to parse. </param>
        /// <returns>
        /// Parsed string in the given format.
        /// </returns>
        public static string ParseCamelCase(string _Input)
        {
            return Regex.Replace(_Input,
                @"(?<=[a-z])(?=[A-Z0-9])|(?<=[A-Z])(?=[A-Z][a-z])|(?<=[A-Za-z])(?=[0-9])", " "
            );
        }
    }
}

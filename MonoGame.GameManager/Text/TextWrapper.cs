using System;
using System.Collections.Generic;
using System.Text;

namespace MonoGame.GameManager.Text
{
    /// <summary>
    /// Splits a text into lines that fit a width (word wrap). It does not depend on a font: the width of a text is
    /// measured by a function, eg: <c>text =&gt; font.MeasureString(text).X</c>.
    /// </summary>
    public static class TextWrapper
    {
        /// <summary>
        /// Wraps a text. Line breaks are kept ("\n", "\r\n" and the two characters "\n" written in data files).
        /// Lines are broken between words; a word longer than the width is broken between characters. Spaces at the
        /// end of the lines are removed.
        /// </summary>
        /// <param name="text">The text (null or empty gives no lines).</param>
        /// <param name="measureWidth">Measures the width of a text.</param>
        /// <param name="maxWidth">The maximum width of a line (infinity or 0 or less = no wrapping).</param>
        public static IList<string> Wrap(string text, Func<string, float> measureWidth, float maxWidth)
        {
            if (measureWidth == null)
                throw new ArgumentNullException(nameof(measureWidth));

            var lines = new List<string>();
            if (string.IsNullOrEmpty(text))
                return lines;

            var normalized = NormalizeLineBreaks(text);
            var wrap = maxWidth > 0f && !float.IsInfinity(maxWidth) && !float.IsNaN(maxWidth);
            foreach (var paragraph in normalized.Split('\n'))
            {
                if (!wrap || paragraph.Length == 0 || measureWidth(paragraph.TrimEnd()) <= maxWidth)
                    lines.Add(paragraph.TrimEnd());
                else
                    WrapParagraph(paragraph, measureWidth, maxWidth, lines);
            }

            return lines;
        }

        /// <summary>Converts "\r\n", "\r" and the escaped sequence "\n" (backslash + n) to '\n'.</summary>
        public static string NormalizeLineBreaks(string text)
            => text.Replace("\r\n", "\n").Replace('\r', '\n').Replace("\\n", "\n").Replace("\\N", "\n");

        private static void WrapParagraph(string paragraph, Func<string, float> measureWidth, float maxWidth, List<string> lines)
        {
            var line = new StringBuilder();
            var index = 0;
            while (index < paragraph.Length)
            {
                // The next token is a word with the spaces before it.
                var tokenStart = index;
                while (index < paragraph.Length && paragraph[index] == ' ')
                    index++;
                while (index < paragraph.Length && paragraph[index] != ' ')
                    index++;
                var token = paragraph.Substring(tokenStart, index - tokenStart);

                if (measureWidth((line + token).TrimEnd()) <= maxWidth)
                {
                    line.Append(token);
                    continue;
                }

                if (line.Length > 0)
                {
                    lines.Add(line.ToString().TrimEnd());
                    line.Clear();
                }

                token = token.TrimStart(); // a new line does not start with spaces

                // A word wider than the line is broken between characters.
                while (token.Length > 0 && measureWidth(token) > maxWidth)
                {
                    var count = CountFittingCharacters(token, measureWidth, maxWidth);
                    lines.Add(token.Substring(0, count));
                    token = token.Substring(count);
                }

                line.Append(token);
            }

            if (line.Length > 0)
                lines.Add(line.ToString().TrimEnd());
        }

        /// <summary>The number of characters of the start of a word that fit the width (at least one).</summary>
        private static int CountFittingCharacters(string word, Func<string, float> measureWidth, float maxWidth)
        {
            int low = 1, high = word.Length;
            while (low < high)
            {
                var middle = (low + high + 1) / 2;
                if (measureWidth(word.Substring(0, middle)) <= maxWidth)
                    low = middle;
                else
                    high = middle - 1;
            }
            return low;
        }
    }
}

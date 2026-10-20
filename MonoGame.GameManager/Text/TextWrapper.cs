using System;
using System.Collections.Generic;
using System.Text;

namespace MonoGame.GameManager.Text
{
    /// <summary>
    /// Splits a text into lines that fit a width (word wrap). It does not depend on a font: the width of a text is
    /// measured by a function, eg: <c>text =&gt; font.MeasureString(text).X</c>.
    /// </summary>
    /// <remarks>
    /// Scripts written without spaces between words (Thai, Lao, Khmer, Myanmar, Chinese, Japanese) may break between any
    /// two letters (<see cref="CanBreakBetween"/>), but never before a combining mark or a following vowel and never after a
    /// leading vowel, so a letter keeps its marks. There is no dictionary of words: the break is between syllables, not
    /// always between words.
    /// </remarks>
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
                // The next token is a word with the spaces before it (in a script without spaces, a cluster of letters).
                var tokenStart = index;
                while (index < paragraph.Length && paragraph[index] == ' ')
                    index++;
                while (index < paragraph.Length && paragraph[index] != ' ')
                {
                    index++;
                    if (index < paragraph.Length && paragraph[index] != ' ' && CanBreakBetween(paragraph[index - 1], paragraph[index]))
                        break;
                }
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

        /// <summary>
        /// True when a line may break between <paramref name="before"/> and <paramref name="after"/> without a space: one of
        /// them is in a script written without spaces, <paramref name="after"/> is no mark or following vowel and
        /// <paramref name="before"/> is no leading vowel.
        /// </summary>
        public static bool CanBreakBetween(char before, char after)
        {
            if (!IsNoSpaceScript(before) && !IsNoSpaceScript(after))
                return false;
            if (IsAttachedToPrevious(after) || IsLeadingVowel(before))
                return false;
            // Punctuation stays with the letter before it ("ไทย!", "日本。").
            if (char.IsPunctuation(after) && !IsNoSpaceScript(after))
                return false;
            return !(char.IsLetterOrDigit(before) && !IsNoSpaceScript(before) && char.IsLetterOrDigit(after) && !IsNoSpaceScript(after));
        }

        /// <summary>Thai, Lao, Myanmar, Khmer, the Japanese kana and the Chinese characters.</summary>
        public static bool IsNoSpaceScript(char c)
            => (c >= '\u0E00' && c <= '\u0EFF')   // Thai, Lao
               || (c >= '\u1000' && c <= '\u109F') // Myanmar
               || (c >= '\u1780' && c <= '\u17FF') // Khmer
               || (c >= '\u3040' && c <= '\u30FF') // Hiragana, Katakana
               || (c >= '\u3400' && c <= '\u9FFF'); // CJK ideographs

        /// <summary>
        /// A mark or a vowel that is drawn on, under or after the letter before it: it can never start a line. Thai: MAI HAN-AKAT,
        /// SARA A / AA / AM, the vowels above and below, the tone marks, LAKKHANGYAO, MAIYAMOK; any Unicode combining mark.
        /// </summary>
        public static bool IsAttachedToPrevious(char c)
        {
            if (c >= '\u0E30' && c <= '\u0E3A')
                return true;
            if (c >= '\u0E45' && c <= '\u0E4E')
                return true;
            if (c >= '\u0EB0' && c <= '\u0EBC' || c >= '\u0EC8' && c <= '\u0ECD')
                return true;
            var category = char.GetUnicodeCategory(c);
            return category == System.Globalization.UnicodeCategory.NonSpacingMark
                || category == System.Globalization.UnicodeCategory.SpacingCombiningMark
                || category == System.Globalization.UnicodeCategory.EnclosingMark;
        }

        /// <summary>A vowel written before the consonant it follows in speech (Thai SARA E to SARA AI MAIMALAI, Lao): never ends a line.</summary>
        public static bool IsLeadingVowel(char c) => (c >= '\u0E40' && c <= '\u0E44') || (c >= '\u0EC0' && c <= '\u0EC4');

        /// <summary>The number of characters of the start of a word that fit the width (at least one).</summary>
        private static int CountFittingCharacters(string word, Func<string, float> measureWidth, float maxWidth)
        {
            var count = CountFittingCharactersRaw(word, measureWidth, maxWidth);
            // Never leave a mark at the start of the next line.
            while (count > 1 && count < word.Length && IsAttachedToPrevious(word[count]))
                count--;
            return count;
        }

        private static int CountFittingCharactersRaw(string word, Func<string, float> measureWidth, float maxWidth)
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


using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SBSSData.Softball.Common
{
    /// <summary>
    /// Extensions used by the <c>Levaro.Softball</c> API that supports the construction of SBSSDataBuilder application.
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Returns a substring starting from the <paramref name="start"/> string parameter and ending at the <paramref name="end"/>
        /// parameter.
        /// </summary>
        /// <param name="content">The string from which the substring is returned. If <c>null</c> or empty, the empty string
        /// is returned.</param>
        /// <param name="start">The returned substring starts at the location of the value of this parameter.</param>
        /// <param name="end">The returned substring ends at the location of the value of this parameter.</param>
        /// <param name="includeStart">
        ///     If <c>true</c>, the <paramref name="start"/> string is included in the returned substring; otherwise the returned 
        ///     substring begins immediated after the <c>start</c> string. The default is false.
        /// </param>
        /// <param name="includeEnd">
        ///     If <c>true</c>, the <paramref name="end"/> string is included in the returned substring; otherwise the returned 
        ///     substring ends at the character immediately before the <c>end</c> string. The default is true.
        /// </param>
        /// <returns>
        ///     A substring based upon the values of the parameters. If the source (<paramref name="content"/>), 
        ///     <paramref name="start"/>, <paramref name="end"/> are <c>null</c> or empty, the empty string is returned. 
        ///     The empty string is also returned if an <c>ArgumentOutOfRangeException</c> object is thrown; this can happen 
        ///     if the <c>start</c> string does not appear before the <c>end</c> string. <c>null</c> is never returned.
        /// </returns> 
        public static string Substring(this string content, string start, string end, bool includeStart = false, bool includeEnd = true)
        {
            string substring = string.Empty;
            if (!(string.IsNullOrEmpty(content) || string.IsNullOrEmpty(start) || string.IsNullOrEmpty(end)))
            {
                try
                {
                    int startIndex = content.IndexOf(start);
                    if (!includeStart)
                    {
                        startIndex += start.Length;
                    }

                    int endIndex = content.IndexOf(end, startIndex);
                    int length = endIndex - startIndex;
                    if (includeEnd)
                    {
                        length += end.Length;
                    }

                    substring = content.Substring(startIndex, length);
                }
                catch (ArgumentOutOfRangeException)
                {
                    substring = string.Empty;
                }
            }

            return substring;
        }

        /// <summary>
        /// Returns a substring beginning at specified text (the <paramref name="start"/>) until the end of the 
        /// <paramref name="content"/>.
        /// </summary>
        /// <param name="content">The string whose substring is returned.</param>
        /// <param name="start">The returned substring starts at the location of the value of this parameter.</param>
        /// <param name="includeStart">
        /// If <c>true</c>, the <paramref name="start"/> string is included in the returned substring; otherwise the returned 
        /// substring begins immediately after the <c>start</c> string. The default is false.
        /// </param>
        /// <returns>A substring of the <paramref name="content"/> string based upon the values of the other parameters.</returns>
        /// <seealso cref="Substring(string, string, string, bool, bool)"/>.
        public static string Substring(this string content, string start, bool includeStart = false)
        {
            return Substring(content + "~", start, "~", includeStart, false);
        }

        /// <summary>
        /// Formats and returns the string using comma delimited or decimal values using KB and MB when appropriate.
        /// </summary>
        /// <param name="size">The source for this extension and is formatted based on parameter settings.</param>
        /// <param name="simple">If true, of the value of <paramref name="size"/> is the string returned is comma delimited. If
        /// not true, then the value is formatted using KB or MB which ever is more appropriate. This parameter is optional and
        /// the default value is <c>false</c>.</param>
        /// <param name="extend">If <c>true</c> and the format is not simple, then the text is extended beyond KB or MB values, 
        /// with the comma delimited text. If <paramref name="simple"/>is <c>true</c> this value is ignored. It is optional and the
        /// default value is <c>false</c>".
        /// </param>
        /// <returns>
        /// The text for the value of <paramref name="size"/>. For example,
        /// <code language="cs" title="Sample Results">
        /// 999.FormatInt().ToString() returns "999"
        /// 1037.FormatInt() yields 1.04 KB
        /// 11178.FormatInt() yields 11.2 KB
        /// 971623.FormatInt(false, true) yields 971.6 KB (971,623 bytes)
        /// 2124599.FormatInt() yields 2.12 MB
        /// 2124599.FormatInt(false, true) yields 2.12 MB (2,124,599 bytes)
        /// 97324456.FormatInt(true) yields 97,324,456 bytes
        /// 97324456.FormatInt() yields 97.32 MB
        /// </code>
        /// </returns>
        public static string FormatInt(this int size, bool simple = false, bool extend = false)
        {
            double altSize = (double)size;
            string format = "#,##0 bytes";
            string formattedInt = altSize.ToString(format);
            if (!simple)
            {
                if ((size > 999) && (size <= 9999))
                {
                    altSize /= 1000.0;
                    format = "#,###.00 KB";
                }
                else if ((size > 9999) && (size <= 999999))
                {
                    altSize /= 1000.0;
                    format = "#,###.0 KB";
                }
                else if (size > 999999)
                {
                    altSize /= 1000000.0;
                    format = "#,###.00 MB";
                }
                formattedInt = altSize.ToString(format);

                if (extend & (size > 999))
                {
                    string baseFormat = " (#,##0 bytes)";
                    string baseFormattedInt = size.ToString(baseFormat);
                    formattedInt += baseFormattedInt;
                }
            }

            return formattedInt;
        }


        [GeneratedRegex(@"\s+")]
        private static partial Regex NoWhiteSpaceRegex();

        /// <summary>
        /// Returns the <paramref name="content"/> string with all white space removed using the source generated 
        /// <see cref="Regex"/>.
        /// </summary>
        /// <remarks>
        /// The <c>Regex</c> instance is generated from the expression <code language="cs">"\\s+"</code>.
        /// </remarks>
        /// <param name="content">The string from which to remove all white space</param>
        /// <returns><c>content</c> with all white space removed. The empty string is returned if <c>content</c> is
        /// empty or <c>null</c>.</returns>
        public static string RemoveWhiteSpace(this string content) => NoWhiteSpaceRegex().Replace(content ?? string.Empty, "");

        [GeneratedRegex("[\\s*]\\([a-z|A-Z|1-9]*\\)")]
        private static partial Regex RemoveParentheticalText();

        /// <summary>Decodes HTML encoded characters (for example <![CDATA[&lt;]]> to &lt;) and removes all leading text 
        /// beginning with zero or more white 
        /// space characters followed by alphanumeric characters surrounded by parenthesis. Also all leading and trailing white 
        /// space is removed. This rather specialized method is used to "normalize" names recovered from data scraped 
        /// from HTML pages.
        /// </summary>
        /// <remarks>
        /// The <see cref="Regex"/> instance used is generated using the regular expression 
        /// <code>"[\\s*]\\([a-z|A-Z|1-9]*\\)"</code>
        /// </remarks>
        /// <param name="name">The text to modify. If empty or <c>null</c>, the empty string is returned.</param>
        /// <returns>
        /// The modified string by (1) decoding any HTML characters, (2) removing any text using the <c>RemoveParentheticalText</c>
        /// generated <c>Regex</c> object, (3) removing leading and trailing white space characters. If <paramref name="name"/>
        /// is empty or <c>null</c> the empty string is returned.
        /// </returns>
        public static string CleanNameText(this string name)
        {
            string teamName = System.Net.WebUtility.HtmlDecode(name);
            teamName = RemoveParentheticalText().Replace(teamName ?? string.Empty, string.Empty).Trim();
            return teamName;
        }

        /// <summary>
        /// Splits a string and "swaps" the first two capitalized items.
        /// </summary>
        /// <remarks>
        /// Names are often in the form "lastName", "firstName" (often to make sorting easier). This extension splits the string
        /// and return a string more suitable for display. For example "corley,    malcolm" is returned as
        /// "Malcolm Corley". Notice that excess white space is removed, and the strings are capitalized if needed.
        /// </remarks>
        /// <param name="source">The source string to split. If it is <c>null</c> or empty, the empty string is returned.</param>
        /// <param name="delimiter">The character to use to split the string. This parameter is option and default value
        /// is a comma (',').</param>
        /// <returns>
        /// The capitalized swapped items if there are two non-empty items. If there is just one item, it is returned. if the
        /// <paramref name="source"/> is <c>null</c> or empty, the empty string is returned. If there are more than two items
        /// after splitting the <c>source</c>, only the first two are used to construct the returned string. <c>null</c> is
        /// never returned.
        /// </returns>
        public static string BuildDisplayName(this string source, char delimiter = ',')
        {
            string displayName = string.Empty;
            if (!string.IsNullOrEmpty(source))
            {

                string firstName = ((source.Split(delimiter).Length >= 2) ?
                                     source.Split(delimiter)[1].Trim() :
                                     string.Empty).Capitalize();
                string lastName = source.Split(delimiter)[0].Trim().Capitalize();
                displayName = (string.IsNullOrEmpty(lastName) ? firstName : $"{firstName} {lastName}").Trim();
            }

            return displayName;
        }

        /// <summary>
        /// The first character of the string is changed to upper case.
        /// </summary>
        /// <param name="content">The string whose first character is capitalized.</param>
        /// <returns>The capitalized string; if <paramref name="content"/>is <c>null</c> or empty, the empty string is
        /// returned.</returns>
        public static string Capitalize(this string content)
        {
            string word = string.Empty;
            if (!string.IsNullOrWhiteSpace(content))
            {
                char[] characters = content.ToCharArray();
                characters[0] = char.ToUpper(characters[0]);
                word = new string(characters);
            }

            return word;
        }

        /// <summary>
        /// Converts the string to a sequence of words
        /// </summary>
        /// <remarks>A word is a sequence of characters beginning with an uppercase letter and the previous letter is is not 
        /// upper case. For example, "HelloRichard" returns the sequence "Hello", "Richard". This is often useful to create
        /// words associated with file names (specifically image names) that do not use spaces.
        /// </remarks>
        /// <param name="content">The string to convert. If it is <c>null</c> or the empty string, the empty sequence is
        /// returned.</param>
        /// <returns>
        /// A sequence of words, each of which starts with a capital letter.
        /// </returns>
        public static IEnumerable<string> NameToWords(this string content)
        {
            List<string> words = [];
            if (!string.IsNullOrWhiteSpace(content))
            {
                List<char> wordChars = [];
                char prevChar = ' ';
                for (int i = 0; i < content.Length; i++)
                {
                    char c = content[i];
                    if (i == 0)
                    {
                        wordChars.Add(c);
                    }
                    else
                    {
                        if (char.IsUpper(c) && !char.IsUpper(prevChar))
                        {
                            words.Add(new string(wordChars.ToArray()));
                            wordChars = [];
                        }

                        wordChars.Add(c);
                    }

                    prevChar = c;
                }

                if (wordChars.Count > 0)
                {
                    words.Add(new string(wordChars.ToArray()));
                }
            }

            return words;
        }

        /// <summary>
        /// Converts a string to words from a list of words and to a string with each separated by a single space.
        /// </summary>
        /// <remarks>
        /// The <see cref="NameToWords(string)"/> method to creates a sequence of words, and then the 
        /// <see cref="ToString{T}(IEnumerable{T}, string)"/> method to paste the words together using a single space between
        /// each word. Words not at the beginning of the sequence and are certain preposition are not capitalized.
        /// </remarks>
        /// <param name="name">The text to convert to a string of words with a single space between each word</param>
        /// <returns>A string of space delimited words. The empty string is returned if <paramref name="name"/> is <c>null</c>
        /// or empty.</returns>
        public static string NameToTitle(this string name)
        {
            List<string> lowerCase =
            [
                "Of",
                "Off",
                "A",
                "An",
                "To",
                "From",
                "The",
                "At",
                "And",
                "Or"
            ];

            List<string> words = (name.NameToWords()).ToList();
            List<string> titleWords = [words.First()];

            foreach (string word in words.Skip(1))
            {
                string titleWord = word;
                if (lowerCase.Any(w => string.Equals(w, word, StringComparison.OrdinalIgnoreCase)))
                {
                    titleWord = word.ToLower();
                }

                titleWords.Add(titleWord);
            }

            return titleWords.ToString<string>(" ");
        }

        /// <summary>
        /// Converts a byte array to a UTF8 string.
        /// </summary>
        /// <param name="byteArray">The byte array each entry of which is a returned character in the string</param>
        /// <returns>The UTF8, or the empty string if the <paramref name="byteArray"/> is <c>null</c> or empty.</returns>
        public static string ByteArrayToString(this byte[] byteArray)
        {
            return (byteArray != null) ? Encoding.UTF8.GetString(byteArray, 0, byteArray.Length) : string.Empty;
        }

        /// <summary>
        /// Convert an array of bytes to a Base64 string.
        /// </summary>
        /// <param name="byteArray">The array of bytes.</param>
        /// <returns>
        /// The Base64 string if the <paramref name="byteArray"/> is not null; 
        /// otherwise the empty string is returned
        /// </returns>
        public static string ByteArrayToBase64String(this byte[] byteArray)
        {
            return (byteArray != null) ? Convert.ToBase64String(byteArray) : string.Empty;
        }

        /// <summary>
        /// Create an HTML image (<c>&lt;mg&gt;</c>) tag text from the byte data of the contents of the specified file.
        /// </summary>
        /// <remarks>
        /// The create image tag using the data specification for the image which means rendering the image does not require
        /// a HTML request. This is useful for create stand-alone files that have embedded images.
        /// </remarks>
        /// <param name="filePath">
        /// The full file path of the image to use to create image tag.
        /// </param>
        /// <param name="pixels">
        /// The number of pixels of the image to be displayed. This is done by inserting a <c>style</c>
        /// attributes in the return tag text. The parameter and is not provided a value of 0 is used and no <c>style</c>
        /// attribute is inserted in the returned tag text.
        /// </param>
        /// <param name="imageTypeName">
        /// The type of the image and must be one of the values from the list:
        /// <code language="cs">
        /// List{string} imageTypes = ["jpg", "jpeg", "jpe", "bmp", "gif", "png"];
        /// </code>
        /// This parameter is optional and if not specified the file extension is used (and must one of the image types.
        /// </param>
        /// <returns>An image tag text or the empty string if it the file does not exist or the <paramref name="imageTypeName"/>
        /// is not valid. for values of 175 for <paramref name="pixels"/> and <paramref name="imageTypeName"/> of "jpg", the
        /// return image tag is
        /// <code language="html">
        /// <img style="175px;" src="data:image/jpg;base64,/9j/4AA.....Dr6/2Q=="/>
        /// </code>
        /// </returns>
        public static string FileImageToHtml(this string filePath, int pixels = 0, string imageTypeName = "")
        {
            string imgTag = string.Empty;
            if (File.Exists(filePath))
            {
                string sizeStyle = pixels > 0 ? $" style=\"height:{pixels}px\"" : string.Empty;
                List<string> imageTypes = ["jpg", "jpeg", "jpe", "bmp", "gif", "png"];
                string imageType = !string.IsNullOrEmpty(imageTypeName) ? imageTypeName : Path.GetExtension(filePath);
                if (!string.IsNullOrEmpty(imageType))
                {
                    if (imageType.StartsWith('.'))
                    {
                        imageType = imageType[1..];
                    }

                    if (imageTypes.Contains(imageType.ToLower()) && File.Exists(filePath))
                    {
                        byte[] bytes = File.ReadAllBytes(filePath);
                        string base64 = Convert.ToBase64String(bytes);
                        imgTag = $"<img{sizeStyle} src=\"data:image/{imageType};base64,{base64}\"/>";
                    }
                }
            }
            return imgTag;
        }

        /// <summary>
        /// Returns an image embedded as a resource to an image tag having "data:image" source representing the image.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resourceName"></param>
        /// <param name="pixels"></param>
        /// <param name="imageTypeName"></param>
        /// <returns></returns>
        public static string EmbeddedImageResourceToHtml<T>(this string resourceName, int pixels = 0, string imageTypeName = "png")
        {
            string imgTag = string.Empty;
            Assembly assembly = typeof(T).Assembly;
            string[] names = assembly.GetManifestResourceNames();
            string formattedResource = assembly.FormatResourceName(resourceName);
            if (names.Contains(formattedResource))
            {
                byte[] resource = assembly.GetEmbeddedResourceAsBytes(formattedResource);
                string base64 = resource.ByteArrayToBase64String();
                string sizeStyle = pixels > 0 ? $" style=\"height:{pixels}px\"" : string.Empty;
                imgTag = $"<img{sizeStyle} src=\"data:image/{imageTypeName};base64,{base64}\"/>";
            }

            return imgTag;
        }

        /// <summary>
        /// Serializes an object to a JSON string.
        /// </summary>
        /// <param name="source">The object to serialize; if <c>null</c>, the empty string is returned.</param>
        /// <returns>A JSON string representing the object that can deserialized to create an instance of the object. The
        /// string is formatted for indentation using space characters (rather than tabs).</returns>
        public static string ToJsonString(this object source)
        {
            string jsonString = string.Empty;

            if (source != null)
            {
                // stringWriter is disposed when textWriter is disposed.
                StringWriter stringWriter = new();
                using JsonTextWriter textWriter = new(stringWriter);
                textWriter.Formatting = Newtonsoft.Json.Formatting.Indented;
                textWriter.Indentation = 4;
                textWriter.IndentChar = ' ';

                JsonSerializer serializer = new()
                {
                    TypeNameHandling = TypeNameHandling.Auto
                };
                serializer.Serialize(textWriter, source);

                jsonString = stringWriter.GetStringBuilder().ToString();
            }

            return jsonString;
        }

        /// <summary>
        /// Deserializes a sequence of objects from a JSON string.
        /// </summary>
        /// <typeparam name="T">The type of the elements of the sequence</typeparam>
        /// <param name="source">The JSON string that is deserialized.</param>
        /// <returns>A <c>List<typeparamref name="T"/></c></returns>
        public static List<T> FromJsonString<T>(this string source)
        {
            return (List<T>)(JsonConvert.DeserializeObject<List<T>>(source) ?? Enumerable.Empty<T>());
        }

        /// <summary>
        /// Deserializes an object from the contents of a specified file path.
        /// </summary>
        /// <typeparam name="T">The file path from which a JSON is read and used to deserialize and object of type <typeparamref name="T"/>
        /// </typeparam>
        /// <param name="sourceFilePath">The source file which is contains the JSON string.</param>
        /// <returns>The default value of T or <c>null</c> if the file does not exist.</returns>
        /// <exception cref="JsonSerializationException">if the string read from the file is not a valid JSON representation
        /// of the desired object.</exception>
        public static T? Deserialize<T>(this string sourceFilePath)
        {
            T? deserializedObject = default;
            if (File.Exists(sourceFilePath))
            {
                string json = File.ReadAllText(sourceFilePath);

                JsonSerializerSettings settings = new()
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                };

                deserializedObject = JsonConvert.DeserializeObject<T>(json, settings);
            }

            return deserializedObject;
        }

        /// <summary>
        /// Serializes the object as JSON text to the file who path <paramref name="filePath"/> is specified. T
        /// </summary>
        /// <remarks>
        /// The object is serialized to a JSON string using the <see cref="ToJsonString(object)"/> and then written to
        /// the specified file, which is overwritten if it exists.
        /// </remarks>
        /// <typeparam name="T">The type of the file</typeparam>
        /// <param name="source">An instance of the <typeparamref name="T"/> type</param>
        /// <param name="filePath">
        /// The full path of the file where the JSON string is written. This should be a full path, and if the directory
        /// portion of the path does not exist, it is created. If the file already exists, it is overwritten.
        /// </param>
        /// <returns>
        /// The number of characters written to the file. If <paramref name="source"/> is <c>null</c> or 
        /// <paramref name="filePath"/> is <c>null</c> or empty, the file is not created and -1 is returned.
        /// </returns>
        public static int Serialize<T>(this T source, string filePath)
        {
            int length = -1;
            if ((source != null) && !string.IsNullOrEmpty(filePath))
            {
                string directory = Path.GetDirectoryName(filePath) ?? string.Empty;
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = source.ToJsonString();
                File.WriteAllText(filePath, json);
                length = json.Length;
            }

            return length;
        }

        /// <summary>
        /// Formats a JSON string using the <c>JsonTextWriter</c> used in <see cref="ToJsonString(object)"/>.
        /// </summary>
        /// <param name="source">The JSON text to format</param>
        /// <returns>
        /// The formatted JSON text or the empty string if the <paramref name="source"/> is <c>null</c> or not a valid
        /// JSON string.
        /// </returns>
        /// <seealso cref="ToJsonString(object)"/>
        public static string FormatJsonString(this string source)
        {
            string jsonText = string.Empty;
            if (!string.IsNullOrEmpty(source))
            {
                jsonText = JToken.Parse(source).ToJsonString();
            }

            return jsonText;
        }


        /// <summary>
        /// Converts an sequence of objects of type <typeparamref name="T"/> to a string where each element of the sequence
        /// is converted to a string and concatenated using the <paramref name="itemSeparator"/> string.
        /// </summary>
        /// <typeparam name="T">The type of the sequence elements</typeparam>
        /// <param name="source">The sequence if elements</param>
        /// <param name="toStringCallback">A delegate accepting a element of <typeparamref name="T"/> and returning a string for
        /// that element. If <c>null</c>, the default <c>ToString()</c> method for the element object is used.</param>
        /// <param name="itemSeparator">
        /// A string that separates each element of the returned string. The default value is "; " (a semicolon followed by a
        /// space character).
        /// </param>
        /// <returns>
        /// A string with each element of the sequence separated to the <c>itemSeparator</c>. The empty string is returned if 
        /// <paramref name="source"/> is <c>null</c> or empty.
        /// </returns>
        public static string ToString<T>(this IEnumerable<T> source, Func<T, string>? toStringCallback, string itemSeparator = "; ")
        {
            string sourceItems = string.Empty;
            if (source != null)
            {
                Func<T, string?> callback = x => x?.ToString();
                if (toStringCallback != null)
                {
                    callback = toStringCallback;
                }

                sourceItems = string.Join(itemSeparator, source.Select(i => callback(i)));

            }

            return sourceItems;
        }

        /// <summary>
        /// Converts an sequence of objects of type <typeparamref name="T"/> to a string where each element of the sequence
        /// is converted to a string using its <c>ToString()</c> method and concatenated using the 
        /// <paramref name="itemSeparator"/> string.
        /// </summary>
        /// <typeparam name="T">The type of the sequence elements</typeparam>
        /// <param name="source">The sequence if elements</param>
        /// <param name="itemSeparator">
        /// A string that separates each element of the returned string. The default value is "; " (a semicolon followed by a
        /// space character). 
        /// </param>
        /// <returns>
        /// A string with each element of the sequence separated to the <c>itemSeparator</c>. The empty string is returned if 
        /// <paramref name="source"/> is <c>null</c> or empty.
        /// </returns>
        public static string ToString<T>(this IEnumerable<T> source, string itemSeparator = "; ")
        {
            string sourceItems = string.Empty;
            if (source != null)
            {
                sourceItems = source.ToString<T>(null, itemSeparator);
            }

            return sourceItems;
        }

        /// <summary>
        /// Converts a stack of objects of type <typeparamref name="T"/> to a string where each element of the sequence
        /// is converted to a string using its <c>ToString()</c> method and concatenated using a carriage return/line feed
        /// string.
        /// </summary>
        /// <typeparam name="T">The type of the sequence elements</typeparam>
        /// <param name="source">The stack if elements</param>
        /// <returns>
        /// A string with each element of the stack separated to the <c>CRLF</c>. The empty string is returned if 
        /// <paramref name="source"/> is <c>null</c> or empty.
        /// </returns>
        public static string ToString<T>(this Stack<T> source)
        {
            string stack = string.Empty;
            if (source != null)
            {
                stack = source.ToArray().ToString<T>("\r\n");
            }

            return stack;
        }

        /// <summary>
        /// Parses a string to an <c>int</c>.
        /// </summary>
        /// <param name="source">The string to parse.</param>
        /// <returns>
        /// <c>null</c> if the <paramref name="source"/> cannot be parsed (<c>null</c>, empty or not a valid int
        /// </returns>
        public static int? ParseInt(this string source)
        {
            return int.TryParse(source, out int result) ? result : null;
        }

        /// <summary>
        /// Extension method that returns the assembly version and build data as string formatted using the specified
        /// format string.
        /// </summary>
        /// <remarks>
        /// It is assumed that the build version is of the format "yyddd" where yy is the year and ddd is the number of the
        /// day in the year. For example, 16350 corresponds to the day 350 of 2016 (December 15, 2016).
        /// </remarks>
        /// <param name="source">
        /// The assembly whose version number is returned.
        /// </param>
        /// <param name="format">
        /// A format string that must contain full replaceable parameters ({0}, {1}, {2}, {3}, and {4}). The first is used for
        /// the build date and {1} and {2} are used for version major and minor numbers and {3} and {4} are used for the build
        /// and version revision numbers. The replaceable parameters can be in any order and use any format, but there must be five.
        /// </param>
        /// <returns>
        /// The full version string (all four components) is returned together with the full date corresponding to the
        /// build version formatted using the <paramref name="format"/> string. The empty string is returned if
        /// <paramref name="source"/> is <c>null</c>.
        /// </returns>
        public static string GetFullVersion(this Assembly source, string format)
        {
            string versionStr = string.Empty;
            if (source != null)
            {
                DateTime date = DateTime.Now.Date; ;
                Version version = source.GetName().Version ?? new Version(1, 0, date.Year * 1000 + date.DayOfYear);
                int build = version.Build;
                int year = 2000 + (build / 1000);
                int days = (build % 1000) - 1;
                DateTime buildDate = new(year, 1, 1);
                buildDate = buildDate.AddDays(days);

                versionStr = string.Format(format, buildDate, version.Major, version.Minor, build, version.Revision);
            }

            return versionStr;
        }

        /// <summary>
        /// For the specified <paramref name="assembly"/>, the value of the property for a custom attribute of 
        /// <typeparamref name="T"/> is returned
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <param name="assembly">The assembly whose attributes are inspected.</param>
        /// <param name="propertyName">The property of the attribute whose value is returned.</param>
        /// <returns>The property value cast as a string for the recovered property. The empty string is returned if 
        /// <paramref name="assembly"/> is <c>null</c> or <paramref name="propertyName"/> is <c>null</c> or empty, or if
        /// the attributes or property or value cannot be recovered.
        /// </returns>
        public static string GetAttribute<T>(this Assembly assembly, string propertyName)
        {
            string attributeValue = string.Empty;
            if ((assembly != null) && !string.IsNullOrEmpty(propertyName))
            {
                Attribute? attribute = assembly.GetCustomAttribute(typeof(T));
                PropertyInfo? property = attribute?.GetType()?.GetProperty(propertyName);
                string? value = property?.GetValue(attribute) as string;
                attributeValue = value ?? string.Empty;
            }

            return attributeValue;
        }

        /// <summary>
        /// Formats a common resource name (as in Visual Studio) into a formatted name that can be found when recovering
        /// the resource from the assembly.
        /// </summary>
        /// <param name="assembly">The assembly in which the resource is embedded.</param>
        /// <param name="resourceName">The common resource name.</param>
        /// <returns>The formatted resource name: it is the assembly name followed by a '.' character and then the
        /// resource name where spaces are replaced by underscores, back and forward slashes are replaced by the
        /// '.' character. If the <paramref name="assembly"/> or <paramref name="resourceName"/> are null or empty,
        /// the empty string is returned.</returns>
        public static string FormatResourceName(this Assembly assembly, string resourceName)
        {
            string formattedResourcedName = string.Empty;
            if ((assembly != null) && !string.IsNullOrEmpty(resourceName))
            {
                formattedResourcedName = assembly.GetName().Name + "." + resourceName.Replace(" ", "_")
                                                                                     .Replace("\\", ".")
                                                                                     .Replace("/", ".");
            }

            return formattedResourcedName;
        }

        /// <summary>
        /// Reads the embedded resource and returns it as an array of bytes.
        /// </summary>
        /// <param name="assembly">The assembly in  which the resource is embedded.</param>
        /// <param name="formattedResourceName">
        /// The formatted resource name, <see cref="FormatResourceName(Assembly, string)"/>
        /// </param>
        /// <returns>The array of bytes of the embedded resource. The empty array is returned if the the
        /// <paramref name="assembly"/> is <c>null</c>, the <paramref name="formattedResourceName"/> is <c>null</c>
        /// or empty, or the resource cannot be found.</returns>
        public static byte[] GetEmbeddedResourceAsBytes(this Assembly assembly, string formattedResourceName)
        {
            byte[] content = [];
            if ((assembly != null) && !string.IsNullOrEmpty(formattedResourceName))
            {
                using Stream? resourceStream = assembly.GetManifestResourceStream(formattedResourceName);
                if (resourceStream != null)
                {
                    content = new byte[resourceStream.Length];
                    _ = resourceStream.Read(content, 0, content.Length);
                }
            }

            return content;
        }

        /// <summary>
        /// Appends text to the end of a file name (and before the extension).
        /// </summary>
        /// <param name="source">The full file path</param>
        /// <param name="appendText">The text to append to the file path before the extension.</param>
        /// <returns>The newly appended full file path. If the text cannot be appended, the original file path (even if
        /// <c>null</c> empty) is returned.</returns>
        public static string AppendTextToFileName(this string source, string appendText)
        {
            string filePath = source;
            string newFilePath = source;

            if (!string.IsNullOrWhiteSpace(source))
            {
                try
                {
                    FileInfo fileInfo = new(source);
                    string extension = fileInfo.Extension;
                    string filePathWithoutExtension = string.IsNullOrEmpty(extension) ? filePath : filePath[..filePath.IndexOf(extension)];
                    string text = appendText ?? string.Empty;
                    newFilePath = $"{filePathWithoutExtension}{text}{extension}";
                }
                catch (ArgumentException exception)
                {
                    newFilePath += $" [{exception.Message}]";
                }
            }

            return newFilePath;
        }

        /// <summary>
        /// Appends the current data and time at the end of the file name of a full file path.
        /// </summary>
        /// <param name="source">The full file path</param>
        /// <param name="format">The format used to append the current time stamp to the end of the filename. The default
        /// format is "-MM-dd-yyyy HH-mm-ss.ff"</param>
        /// <returns>The updated full file path. If the <paramref name="source"/> is <c>null</c> or empty, the empty string
        /// is returned.</returns>
        /// <seealso cref="AppendTextToFileName(string, string)"/>
        public static string AppendTimeStampToFileName(this string source, string format = "-MM-dd-yyyy HH-mm-ss.ff")
        {
            return source.AppendTextToFileName(DateTime.Now.ToString(format));
        }

        /// <summary>
        /// Constructs a <c>DescriptiveStatistics</c> object for the specified sequence of <c>double</c> items.
        /// </summary>
        /// <param name="source">The sequence of items of type <c>double</c></param>
        /// <param name="title">An optional title. The default is just "Statistics for [count] items" where [count] is
        /// the number of items in the <paramref name="source"/> sequence.</param>
        /// <param name="mean">
        /// If <c>null</c> (which is the default), the value is calculated from the <c>source</c> sequence. Otherwise it used
        /// to calculate the variance and sums of squares. Generally this should not be set unless it is part of data
        /// normalization.
        /// </param>
        /// <returns>A <c>DescriptiveStatistics</c> instance. If the <paramref name="source"/> is <c>null</c> or of length
        /// zero, the empty (<c>IsEmpty</c> is <c>true</c>) instance is returned.</returns>
        public static DescriptiveStatistics GetStatistics(this IEnumerable<double> source, string? title = null, double? mean = null)
        {
            return DescriptiveStatistics.GetStatistics(source, title, mean);
        }
    }
}

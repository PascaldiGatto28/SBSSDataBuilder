using HtmlAgilityPack;

namespace SBSSData.Application.Support
{
    /// <summary>
    /// Support extensions and methods for applications when displaying information, particularly in Web pages and
    /// LINQPad queries.
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// Returns a string representing the <see cref="Type"/> name.
        /// </summary>
        /// <remarks>
        /// For simple types, the <see cref="Type.Name"/> property is sufficient, however for generic and anonymous types the
        /// name is not very useful for display. For example, for the following two classes, the display of the class names
        /// <code language="cs">
        /// typeof(Foobar<IEnumerable<int>>).Name;
        /// var x = new
        /// {
        ///    Name = "Richard",
        ///    Y = 23,
        ///    Address = new { Point1 = new Point(0, 0), Point2 = new Point(1, 1) },
        ///    Foobar = new Foobar<IEnumerable<string>>()
        /// };
        /// x.GetType().Name;
        /// </code>
        /// produce the following
        /// <code language="cs">
        /// Foobar`1
        /// <>f__AnonymousType0`4
        /// </code>
        /// where as using the extension method to display the names 
        /// <code language="cs">
        /// x.GetType().TypeToString();
        /// typeof(Foobar<IEnumerable<int>>).TypeToString();
        /// </code>
        /// produce the following
        /// <code language="cs">
        /// Foobar<IEnumerable<Int32>>
        /// Anonymous<String, Int32, Anonymous<Point, Point>, Foobar<IEnumerable<String>>>
        /// </code>
        /// </remarks>
        /// <param name="type">The <see cref="Type"/> source of the extension method, or the first parameter is calling 
        /// directly.</param>
        /// <param name="typeString">An initial string that is appended to as embedded types are encountered. This is an
        /// optional parameter that normally is only used by the method itself during the recursion process if necessary.</param>
        /// <returns>
        /// A readable string representing the name of the type.
        /// </returns>
        public static string TypeToString(this Type type, string typeString = "")
        {
            string display = typeString;
            Type t = type.UnderlyingSystemType;
            bool isAnonymous = t.Name.StartsWith("<>f__AnonymousType");
            string name = isAnonymous ? "Anonymous" : t.Name;
            display += name;
            if (t.IsGenericType && !isAnonymous)
            {
                display = display[..display.IndexOf('`')];
                display = ProcessGenericTypeArgs(display, t);
            }
            else if (isAnonymous)
            {
                display = ProcessGenericTypeArgs(display, t);
            }
            return display;
        }

        /// <summary>
        /// Help method for <see cref="TypeToString"/> extension method to processes type arguments for generic classes.
        /// </summary>
        /// <remarks>
        /// This method calls TypeToString which recursively call this method to handle the case when the type arguments may 
        /// themselves be generic (anonymous classes are considered generic by the C# compiler).
        /// </remarks>
        /// <param name="display">The current readable display name for the parent type and is the return value can be
        /// appended to the processed name.</param>
        /// <param name="t">The <see cref="Type"/> of the generic type argument to process</param>
        /// <returns>The readable display name for the argument type that becomes part of the display name for its parent
        /// type.</returns>
        private static string ProcessGenericTypeArgs(string display, Type t)
        {
            display += "<";

            int length = t.GenericTypeArguments.Length;
            for (int i = 0; i < length; i++)
            {
                if (i > 0)
                {
                    display += ", ";
                }

                Type s = t.GenericTypeArguments[i];
                display = TypeToString(s, display);
                if (i == length - 1)
                {
                    display += ">";
                }
            }

            return display;
        }

        /// <summary>
        /// Returns a string appropriate for the number of times an item occurs.
        /// </summary>
        /// <param name="num">The number of times the item occurs.</param>
        /// <param name="title">The text of the item.</param>
        /// <returns>The <paramref name="num"/> and <paramref name="title"/> as a text string. If <c>num</c> is 0 or negative,
        /// "no [title]s"; if <c>num</c> is 1, "one [title]", if <c>num</c> is any other value, "[num] [title]s" is returned.
        /// </returns>
        public static string NumDesc(this int num, string title)
        {
            string text;
            switch (Math.Max(num, 0))
            {
                case 0:
                {
                    text = $"no {title}s";
                    break;
                }
                case 1:
                {
                    text = $"one {title}";
                    break;
                }
                default:
                {
                    text = $"{num} {title}s";
                    break;
                }
            }

            return text;
        }

        public static bool IsRootTableNode(this HtmlNode tableNode)
        {
            bool isRoot = false;
            if (tableNode != null)
            {
`               isRoot = !tableNode.Ancestors("table").Any();
            }

            return isRoot;
        }

        public static HtmlNode ParentTableNode(this HtmlNode tableNode)
        {
        }

        public static HtmlNode GetSiblingTable(this HtmlNode tableNode, bool nextSibling = true)
        {

        }

    }
}

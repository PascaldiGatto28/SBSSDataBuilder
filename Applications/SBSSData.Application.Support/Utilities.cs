namespace SBSSData.Application.Support
{
    public static class Utilities
    {
        public static string TypeToString(this Type type, string typeString = "")
        {
            string display = typeString;
            Type t = type.UnderlyingSystemType;
            display += t.Name;
            if (t.IsGenericType)
            {
                display = display.Substring(0, display.IndexOf('`'));
                display += "<";

                //foreach (Type s in t.GenericTypeArguments)
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
                    //display += ">";
                }
            }



            return display;
        }

    }
}

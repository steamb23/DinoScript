using System.Text;

namespace DinoScript.Internal;

public static class StringUtils
{
    public static string Escape(string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var builder = ObjectPools.StringBuilderPool.Get();
        try
        {
            foreach (var c in input)
            {
                switch (c)
                {
                    case '\n':
                        builder.Append(@"\n");
                        break;
                    case '\r':
                        builder.Append(@"\r");
                        break;
                    case '\t':
                        builder.Append(@"\t");
                        break;
                    case '\\':
                        builder.Append(@"\\");
                        break;
                    case '"':
                        builder.Append(@"\""");
                        break;
                    case '\0':
                        builder.Append(@"\0");
                        break;
                    default:
                        // 스페이스(공백)는 그대로 유지
                        builder.Append(c);
                        break;
                }
            }

            return builder.ToString();
        }
        finally
        {
            ObjectPools.StringBuilderPool.Return(builder);
        }
    }
}
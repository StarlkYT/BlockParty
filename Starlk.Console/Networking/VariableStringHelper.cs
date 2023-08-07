using System.Text;

namespace Starlk.Console.Networking;

internal static class VariableStringHelper
{
    public static int GetBytesCount(string value)
    {
        var length = Encoding.UTF8.GetByteCount(value);
        return VariableIntegerHelper.GetBytesCount(length) + length;
    }
}
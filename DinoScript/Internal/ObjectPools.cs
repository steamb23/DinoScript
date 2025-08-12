using System.Text;
using Microsoft.Extensions.ObjectPool;

namespace DinoScript.Internal;

internal class ObjectPools
{
    public static readonly ObjectPool<StringBuilder> StringBuilderPool =
        new DefaultObjectPool<StringBuilder>(new StringBuilderPooledObjectPolicy());
}
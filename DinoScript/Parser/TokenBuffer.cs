namespace DinoScript.Parser;

/// <summary>
/// 파서에 전달된 토큰을 버퍼링합니다.
/// </summary>
public class TokenBuffer
{
    /// <summary>
    /// 버퍼에서 토큰을 가져옵니다.
    /// </summary>
    /// <param name="depth"></param>
    /// <returns></returns>
    public Token GetToken(int depth = 0)
    {
        return new Token();
    }
}
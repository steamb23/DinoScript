using System.Data;

namespace DinoScript.Parser;

/// <summary>
/// TextReader의 문자열을 <see cref="TextLength"/> 만큼 가져올 수 있도록 버퍼링합니다.
/// </summary>
public class TextBuffer : IDisposable
{
    /// <summary>
    /// 
    /// </summary>
    public const int TextLength = 1024;
    public const int BufferLength = TextLength * 2;

    private TextReader textReader;
    private char[] buffer = new char[BufferLength];
    private string? text = null;
    
    /// <summary>
    /// 원본에 대한 현재 인덱스
    /// </summary>
    public long Index { get; private set; }
    
    /// <summary>
    /// 원본에 대한 마지막 인덱스
    /// </summary>
    public long LastIndex { get; private set; }

    /// <summary>
    /// 현재 인덱스
    /// </summary>
    private int bufferIndex = 0;

    /// <summary>
    /// 마지막 인덱스
    /// </summary>
    private int bufferLastIndex = 0;

    public TextBuffer(TextReader textReader)
    {
        this.textReader = textReader;
    }

    /// <summary>
    /// 텍스트를 반환할 수 있도록 <see cref="TextReader"/>에서 문자열을 읽어 저장해둡니다.
    /// </summary>
    private void MakeText()
    {
        int count = bufferLastIndex - bufferIndex;
        // count가 1024보다 작을 경우 텍스트 버퍼 작업
        if (count < TextLength)
        {
            int read = 0;

            // lastIndex가 1024보다 큰 상태일 경우 버퍼 재정렬
            if (bufferLastIndex > TextLength)
            {
                Array.Copy(buffer, bufferIndex, buffer, 0, count);
                bufferIndex = 0;
                bufferLastIndex = count;
            }

            read = textReader.Read(buffer, bufferLastIndex, TextLength);

            bufferLastIndex += read;
            LastIndex += read;
            count = bufferLastIndex - bufferIndex;
        }

        text = new string(new ReadOnlySpan<char>(buffer, bufferIndex, Math.Min(count, 1024)));
    }

    /// <summary>
    /// 현재 텍스트를 가져옵니다.
    /// </summary>
    /// <returns></returns>
    public string GetText()
    {
        if (text == null)
        {
            MakeText();
        }

        return text ?? "";
    }

    /// <summary>
    /// 현재 텍스트를 넘기고 다음 텍스트를 가져옵니다.
    /// </summary>
    /// <returns></returns>
    public string NextText()
    {
        Cutout(TextLength);
        return GetText();
    }

    /// <summary>
    /// 버퍼에서 일정 길이 만큼 잘라내어 제외시킵니다. 버퍼가 재정렬됩니다.
    /// </summary>
    /// <param name="length"></param>
    public void Cutout(int length)
    {
        if (text != null)
        {
            var count = Math.Min(length, bufferLastIndex - bufferIndex);
            Index += count;
            bufferIndex += count;
            MakeText();
        }
        else
        {
            Index += length;
            bufferIndex += length;
            MakeText();
        }
    }

    public void Dispose()
    {
        textReader.Dispose();
    }
}
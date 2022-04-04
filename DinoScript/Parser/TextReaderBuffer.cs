using System.Data;

namespace DinoScript.Parser;

public class TextBuffer : IDisposable
{
    private const int WorkLength = 1024;
    private const int BufferLength = WorkLength * 2;

    private TextReader textReader;
    private char[] buffer = new char[BufferLength];
    private string? text = null;
    
    /// <summary>
    /// 원본에 대한 현재 인덱스
    /// </summary>
    public long TotalIndex { get; private set; }
    
    /// <summary>
    /// 원본에 대한 마지막 인덱스
    /// </summary>
    public long TotalLastIndex { get; private set; }

    /// <summary>
    /// 현재 인덱스
    /// </summary>
    private int index = 0;

    /// <summary>
    /// 마지막 인덱스
    /// </summary>
    private int lastIndex = 0;

    public TextBuffer(TextReader textReader)
    {
        this.textReader = textReader;
    }

    private void MakeText()
    {
        int count = lastIndex - index;
        // count가 1024보다 작을 경우 텍스트 버퍼 작업
        if (count < WorkLength)
        {
            int read = 0;

            // lastIndex가 1024보다 큰 상태일 경우 버퍼 재정렬
            if (lastIndex > WorkLength)
            {
                Array.Copy(buffer, index, buffer, 0, count);
                index = 0;
                lastIndex = count;
            }

            read = textReader.Read(buffer, lastIndex, WorkLength);

            lastIndex += read;
            TotalLastIndex += read;
            count = lastIndex - index;
        }

        text = new string(new ReadOnlySpan<char>(buffer, index, Math.Min(count, 1024)));
    }

    public string GetText()
    {
        if (text == null)
        {
            MakeText();
        }

        return text ?? "";
    }

    public void Cutout(int length)
    {
        var count = Math.Min(length, lastIndex - index);
        TotalIndex += count;
        index += count;
        MakeText();
    }

    /// <summary>
    /// 현재 텍스트를 넘기고 다음 텍스트를 가져옵니다.
    /// </summary>
    /// <returns></returns>
    public string NextText()
    {
        Cutout(WorkLength);
        return GetText();
    }

    public void Dispose()
    {
        textReader.Dispose();
    }
}
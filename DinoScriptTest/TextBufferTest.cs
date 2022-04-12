using System.IO;
using System.Linq;
using System.Text;
using DinoScript.Parser;
using Xunit;

namespace DinoScript.Test;

public class TextBufferTest
{
    private readonly string testText;

    public TextBufferTest()
    {
        // 유니코드 완성형 한글 영역으로 데이터 초기화
        var builder = new StringBuilder();
        const int start = 0xac00;
        const int end = 0xd7a3;
        foreach (var i in Enumerable.Range(start, end - start + 1))
        {
            builder.Append((char)i);
        }

        testText = builder.ToString();
    }

    [Fact]
    public void GetTextMethodTest()
    {
        using var stringReader = new StringReader(testText);
        var textBuffer = new TextBuffer(stringReader);

        // 정의된 텍스트 길이 만큼 텍스트를 가져오는지 체크
        var text = textBuffer.GetText();
        Assert.Equal(testText[..TextBuffer.TextLength],text);
    }

    [Fact]
    public void CutoutMethodTest()
    {
        using var stringReader = new StringReader(testText);
        var textBuffer = new TextBuffer(stringReader);

        const int cutoutLength = 3;
        textBuffer.Cutout(cutoutLength);

        var text = textBuffer.GetText();
        Assert.Equal(testText[cutoutLength..(TextBuffer.TextLength + cutoutLength)], text);
    }

    [Fact]
    public void NextTextMethodTest()
    {
        using var stringReader = new StringReader(testText);
        var textBuffer = new TextBuffer(stringReader);

        const int cutoutLength = 3;
        textBuffer.NextText();

        var text = textBuffer.GetText();
        Assert.Equal(testText[TextBuffer.TextLength..(TextBuffer.TextLength + TextBuffer.TextLength)], text);
    }
}
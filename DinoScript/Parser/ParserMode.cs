namespace DinoScript.Parser
{
    public enum ParserMode
    {
        /// <summary>
        /// 전체 구성으로 동작합니다.
        /// </summary>
        Full,
        /// <summary>
        /// 표현식 테스트 모드로 동작합니다.
        /// </summary>
        ExpressionTest,
        /// <summary>
        /// 문장 테스트 모드로 동작합니다.
        /// </summary>
        StatementTest,
    }
}
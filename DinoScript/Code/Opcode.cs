namespace DinoScript.Code
{
    public enum Opcode : byte
    {
        /// <summary>
        /// 아무런 동작도 하지 않습니다.
        /// </summary>
        NoOperation,
        /// <summary>
        /// 상수를 계산 스택에 푸시합니다.
        /// </summary>
        LoadConstantNumber,
        /// <summary>
        /// 계산 스택 맨 위에 있는 값을 제거합니다.
        /// </summary>
        Pop,
        /// <summary>
        /// 계산 스택 맨 위의 값 두 개를 더해서 계산 스택에 푸시합니다.
        /// </summary>
        Add,
        /// <summary>
        /// 계산 스택 맨 위의 값 두 개를 빼서 계산 스택에 푸시합니다.
        /// </summary>
        Subtract,
        /// <summary>
        /// 계산 스택 맨 위의 값 두 개를 곱해서 계산 스택에 푸시합니다.
        /// </summary>
        Multiply,
        /// <summary>
        /// 계산 스택 맨 위의 값 두 개를 나눠서 계산 스택에 푸시합니다.
        /// </summary>
        Divide,
        /// <summary>
        /// 조건 없이 대상 명령 위치로 이동합니다.
        /// </summary>
        Branch,
        /// <summary>
        /// 1바이트 값이 0이 아닌 경우 대상 명령 위치로 이동합니다.
        /// </summary>
        BranchIfTrue,
        /// <summary>
        /// 1바이트 값이 0인 경우 대상 명령 위치로 이동합니다.
        /// </summary>
        BranchIfFalse,
    }
}
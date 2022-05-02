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
        /// 스택 프레임에서 지정된 인덱스의 로컬 변수를 계산 스택에 푸시합니다.
        /// </summary>
        LoadFromLocal,
        /// <summary>
        /// 계산 스택 맨 위에 있는 값을 팝하고 스택 프레임에서 지정된 인덱스의 로컬 변수에 저장합니다.
        /// </summary>
        StoreToLocal,
        /// <summary>
        /// 스택프레임을 생성하고 지정된 함수 심볼이 가리키는 함수를 호출합니다. 호출 정보를 스택에 저장하며 함수의 시작 지점으로 제어가 이동됩니다.
        /// </summary>
        /// <remarks>
        /// 스택에 저장되는 호출 정보는 다음과 같습니다.
        /// - 호출자 제어 위치
        /// - 반환값 유무
        /// </remarks>
        Call,
        /// <summary>
        /// 스택 프레임을 제거하고 호출자로 제어를 반환합니다. 반환값이 있을 경우 값을 푸시합니다.
        /// </summary>
        Return,
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
        /// 조건 없이 대상 명령 위치로 제어를 이동합니다.
        /// </summary>
        Branch,
        /// <summary>
        /// 1바이트 값이 0이 아닌 경우 대상 명령 위치로 제어를 이동합니다.
        /// </summary>
        BranchIfTrue,
        /// <summary>
        /// 1바이트 값이 0인 경우 대상 명령 위치로 제어를 이동합니다.
        /// </summary>
        BranchIfFalse,
    }
}
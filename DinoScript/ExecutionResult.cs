// using DinoScript.Runtime;
//
// namespace DinoScript;
//
// /// <summary>
// /// 가상 머신 스택에서 가장 위에 있는 값을 변환합니다.
// /// </summary>
// public class ExecutionResult
// {
//     const int FullSize = sizeof(long);
//
//     private object? resultObject = null;
//     private double? resultNumber = null;
//
//     // TODO: 추후 스크립트 내에 객체 혹은 테이블 구현이 추가될 경우 적절한 변환 필요
//     public object? ResultObject => resultObject;
//
//     public string? ResultString => resultObject as string;
//
//     public double? ResultNumber => resultNumber;
//
//     /// <summary>
//     /// 가상 메모리에서 결과값을 추출합니다.
//     /// </summary>
//     /// <param name="memory"></param>
//     public ExecutionResult(VirtualMemory memory)
//     {
//         Span<byte> buffer = stackalloc byte[FullSize];
//         var peekedSize = memory.Stack.Peek(buffer);
//
//         Initialize(buffer[..peekedSize], memory);
//     }
//
//     /// <summary>
//     /// 버퍼에서 각 변수를 변환합니다.
//     /// </summary>
//     /// <param name="buffer"></param>
//     public ExecutionResult(Span<byte> buffer)
//     {
//         Initialize(buffer);
//     }
//
//     private void Initialize(Span<byte> buffer, VirtualMemory? memory = null)
//     {
//         // peekedSize가 8 바이트면 가상메모리 주소 참조 시도 및 실수값 저장
//         if (buffer.Length == FullSize)
//         {
//             var int64Result = BitConverter.ToUInt64(buffer);
//             memory?.TryGet(int64Result, out resultObject);
//             // 실수값도 저장
//             resultNumber = BitConverter.ToDouble(buffer);
//         }
//         // 그 외의 값 저장
//     }
// }
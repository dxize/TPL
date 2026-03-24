using Runtime;
using TestsLibrary;
using VirtualMachine.Instructions;

namespace VirtualMachine.UnitTests;

public class HaltTest
{
    [Theory]
    [MemberData(nameof(GetHaltVmData))]
    public void Can_halt_vm_with_exit_code(int exitCode)
    {
        FakeEnvironment environment = new();
        DeaVM vm = new(environment,
        [
            new Instruction(InstructionCode.Push, new Value(exitCode)),
            new Instruction(InstructionCode.Halt),
        ]);

        int result = vm.RunProgram();

        Assert.Equal(exitCode, result);
        Assert.Equal(string.Empty, environment.Output);
    }

    public static TheoryData<int> GetHaltVmData()
    {
        return
        [
            0,
            7,
        ];
    }
}

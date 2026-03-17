using Runtime;

using TestsLibrary;

using VirtualMachine.Builtins;
using VirtualMachine.Instructions;

namespace VirtualMachine.UnitTests;

public class DeaVmTests
{
    [Fact]
    public void Executes_print_and_returns_exit_code_from_halt_value()
    {
        FakeEnvironment environment = new();
        IReadOnlyList<Instruction> instructions =
        [
            new (InstructionCode.Push, new Value("DEA")),
            new (InstructionCode.CallBuiltin, new Value((int)BuiltinFunctionCode.Print)),
            new (InstructionCode.Push, new Value(0)),
            new (InstructionCode.Halt),
        ];

        DeaVM vm = new(environment, instructions);
        int exitCode = vm.RunProgram();

        Assert.Equal(0, exitCode);
        Assert.Equal("DEA", environment.Output);
    }
}
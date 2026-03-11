namespace Lexer.UnitTests;

public class LexicalStatsTest
{
    [Fact]
    public void CollectFromFile_ShouldReturnCorrectStats_ForSampleProgram()
    {
        string code = """
                      var age: int;
                      print("Enter age: ");
                      input(age);

                      if (age >= 18) 
                      {
                          print("Access granted.");
                      } 
                      else 
                      {
                          print("Access denied.");
                      }
                      """;
        
        string tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, code);

        string expectedOutput = """
                                keywords: 7
                                identifier: 4
                                number literals: 1
                                string literals: 3
                                operators: 1
                                other lexemes: 20
                                """;

        try
        {
            string actualOutput = LexicalStats.CollectFromFile(tempFile);

            Assert.Equal(expectedOutput.Replace("\r\n", "\n"), actualOutput.Replace("\r\n", "\n"));
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public void CollectFromFile_ShouldReturnCorrectStats_ForComplexProgram()
    {
        string code = """
                      /* 
                        Calculate sum 
                      */
                      var i: int;
                      var sum: int = 0;
                      
                      for i := 1 to 10 
                      {
                          sum = sum + i; # Add i
                      }
                      print(sum);
                      """;
        string tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, code);

        string expectedOutput = """
                                keywords: 5
                                identifier: 9
                                number literals: 3
                                string literals: 0
                                operators: 4
                                other lexemes: 11
                                """;

        try
        {
            string actualOutput = LexicalStats.CollectFromFile(tempFile);

            Assert.Equal(expectedOutput.Replace("\r\n", "\n"), actualOutput.Replace("\r\n", "\n"));
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }
}

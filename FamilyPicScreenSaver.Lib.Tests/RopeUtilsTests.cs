using FamilyPicScreenSaver.Lib;

namespace FamilyPicScreenSaver.Tests
{
  public class Tests
  {
    [Test]
    public void Randomize_ForEmptyInput_ResultIsEmpty()
    {
      Assert.That(RopeUtils.Randomize<char>("").ToString(), Is.EqualTo(""));
    }

    [Test]
    public void Randomize_ForShortInput_ResultIsAKnownPermutationOfInput()
    {
      for (int i = 0; i < 50; i++)
      {
        Assert.That(RopeUtils.Randomize<char>("abc").ToString(),
          Is.AnyOf("abc", "acb", "bac", "bca", "cab", "cba"));
      }
    }

    [Test]
    public void Randomize_ForShortInput_ResultDoesNotHaveManyRepeats()
    {
      int repeatCount = 0;
      string lastAnswer = null;
      for (int i = 0; i < 50; i++)
      {
        string newAnswer = RopeUtils.Randomize<char>("abc").ToString();
        if (newAnswer == lastAnswer)
        {
          repeatCount++;
        }
        lastAnswer = newAnswer;
      }
      // there is fairly a 1 in 6 (18%) chance for repeat
      // so be generous and assert < 50%
      Assert.That(repeatCount, Is.LessThan(25), "repeat count");
    }
  }
}
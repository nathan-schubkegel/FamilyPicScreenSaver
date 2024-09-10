using FamilyPicScreenSaver.Lib;
using System.Collections.Immutable;
using System.Linq;

namespace FamilyPicScreenSaver.Tests
{
  public class RandomizerTests
  {
    [Test]
    public void Randomize_ForEmptyInput_ResultIsEmpty()
    {
      var list = ImmutableList<char>.Empty;
      list = Randomizer.Randomize(list);
      Assert.That(list, Is.Empty);
    }

    [Test]
    public void Randomize_ForShortInput_ResultIsAKnownPermutationOfInput()
    {
      for (int i = 0; i < 50; i++)
      {
        ImmutableList<char> list = ['a', 'b', 'c'];
        list = Randomizer.Randomize(list);
        Assert.That(new string(list.ToArray()),
          Is.AnyOf("abc", "acb", "bac", "bca", "cab", "cba"));
      }
    }

    [Test]
    public void Randomize_ForShortInput_ResultDoesNotHaveManyRepeats()
    {
      int repeatCount = 0;
      string lastAnswer = null;
      ImmutableList<char> list = ['a', 'b', 'c'];
      for (int i = 0; i < 50; i++)
      {
        list = Randomizer.Randomize(list);
        string newAnswer = new string(list.ToArray());
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
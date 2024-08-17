/*
This is free and unencumbered software released into the public domain under The Unlicense.
You have complete freedom to do anything you want with the software, for any purpose.
Please refer to <http://unlicense.org/>
*/

using Rope;

namespace FamilyPicScreenSaver
{
  public static class RopeUtils
  {
    public static Rope<char> HeuristicToPerfect(Rope<char> heuristic, string perfect)
    {
      // reality check - take as many leading characters as matched
      int sameLength = 0;
      foreach (char c in heuristic)
      {
        if (sameLength == perfect.Length)
        {
          break;
        }
        else if (c == perfect[sameLength])
        {
          sameLength++;
        }
        else
        {
          break;
        }
      }
      return heuristic.Slice(0, sameLength) + perfect.Substring(sameLength);
    }
  }
}

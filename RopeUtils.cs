/*
This is free and unencumbered software released into the public domain under The Unlicense.
You have complete freedom to do anything you want with the software, for any purpose.
Please refer to <http://unlicense.org/>
*/

using Rope;
using System;

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

    [ThreadStatic]
    private static Random _rando;

    public static Rope<T> Randomize<T>(Rope<T> input) where T : IEquatable<T>
    {
      if (_rando == null)
      {
        _rando = new Random();
      }
      var rando = _rando;

      int n = input.Count;
      while (n > 1)
      {
        n--;
        int k = rando.Next(n + 1);
        T value = input[k];
        input = input.SetItem(k, input[n]).SetItem(n, input[k]);
      }

      return input;
    }

    public static Rope<T> SelectMany<T>(Rope<Rope<T>> input) where T : IEquatable<T>
    {
      Rope<T> result = Rope<T>.Empty;
      foreach (var r in input)
      {
        result += r;
      }
      return result;
    }
  }
}

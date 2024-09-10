/*
This is free and unencumbered software released into the public domain under The Unlicense.
You have complete freedom to do anything you want with the software, for any purpose.
Please refer to <http://unlicense.org/>
*/

using System;
using System.Collections.Immutable;

namespace FamilyPicScreenSaver.Lib
{
  public static class Randomizer
  {
    [ThreadStatic]
    private static Random? _rando;

    public static ImmutableList<T> Randomize<T>(ImmutableList<T> input)
    {
      if (_rando == null)
      {
        _rando = new Random();
      }
      var rando = _rando;

      var inputBuilder = input.ToBuilder();
      int n = inputBuilder.Count;
      while (n > 1)
      {
        n--;
        int k = rando.Next(n + 1);
        T value = inputBuilder[k];
        inputBuilder[k] = inputBuilder[n];
        inputBuilder[n] = value;
      }
      return inputBuilder.ToImmutable();
    }
  }
}

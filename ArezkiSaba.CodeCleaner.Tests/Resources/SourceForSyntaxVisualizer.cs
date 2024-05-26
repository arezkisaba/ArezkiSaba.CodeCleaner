using System;
using System.Drawing;
using Windows;
using Windows.Storage;
using ArezkiSaba.CodeCleaner;
using ArezkiSaba.CodeCleaner.Tests;
using static System.Math;
using NativeHttpClient = System.Net.Http.HttpClient;

namespace ArezkiSaba.CodeCleaner.Tests;

public sealed class TestClass1<T>
{
    public TestClass1(
        string arg1,
        string arg2)
    {
        new string(
            ['a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a'],
            0,
            0
        );
        var variable3 = new[]
        {
            new string(
                ['a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a'],
                0,
                0
            ),
            new string(
                ['b', 'b', 'b', 'b', 'b', 'b', 'b', 'b', 'b', 'b', 'b', 'b', 'b', 'b', 'b', 'b', 'b', 'b', 'b', 'b', 'b'],
                0,
                0
            )        };
    }
}

using ArezkiSaba.CodeCleaner.Tests;
using System.Drawing;
using System;
using static System.Math;
using Windows.Storage;
using ArezkiSaba.CodeCleaner.Tests;
using NativeHttpClient = System.Net.Http.HttpClient;
using ArezkiSaba.CodeCleaner;
using System;
using Windows;


namespace ArezkiSaba.CodeCleaner.Tests;

public class TestClass2 : TestClass1<int>
{
    public TestClass2(
        int parameter1,
        int parameter2)
    {
        RuleFor(
            bs => bs.Beneficiaires
        ).SetAaaaaaaaaaaa(
            "argument1",
            Alleeeeeez().Vous().Faire().Foutre()
        ).SetBbbbbbbbbbb(
            "argument1",
            "argument2"
        );
        var variable1 = Method1(
            "Method1Argument1",
            Method2(
                "Method2Argument1",
                Method3(
                    "Method3Argument1",
                    "Method3Argument2"
                )
            ),
            new TestClass1(
                "TestClass1Argument1",
                "TestClass1Argument2",
                new[]
                {
                    "Value1",
                    "Value2"
                }
            )
            {
                Property1 = "Value1",
                Property2 = "Value2"
            },
            new
            {
                Property1 = "Value1",
                Property2 = new[]
                {
                    "Value1",
                    "Value2"
                }
            }
        );
        if (true)
        {
            Console.WriteLine("test");
        }
        for (var i = 0; i < 10; i++)
        {
            Console.WriteLine(i);
        }
        var i = 0;
        while (i < 10)
        {
            Console.WriteLine(i);
            i++;
        }
    }
}

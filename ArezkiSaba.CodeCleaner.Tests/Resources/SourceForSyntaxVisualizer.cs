﻿using System;
using System.Drawing;
using Windows;
using Windows.Storage;
using ArezkiSaba.CodeCleaner;
using ArezkiSaba.CodeCleaner.Tests;
using static System.Math;
using NativeHttpClient = System.Net.Http.HttpClient;

namespace ArezkiSaba.CodeCleaner.Tests;

public class TestClass1<T>
{
    private const string SomeConstField = "SOME CONST VALUE";
    private static string SomeStaticField;
    private readonly string _readonlyStringField1;
    private bool _nonReadonlyBooleanField;

    public event EventHandler SomeEventTriggered;
    public event EventHandler SomeOtherEventCamelCasedTriggered;

    public bool CanDoSomeOtherThing { get; set; }

    private bool _canDoSomething;
    public bool CanDoSomething
    {
        get
        {
            return _canDoSomething;
        }
        set
        {
            _canDoSomething = value;
        }
    }

    private bool _canDoSomethingElse;
    public bool CanDoSomethingElse
    {
        get
        {
            return _canDoSomethingElse;
        }
        set
        {
            _canDoSomethingElse = value;
        }
    }

    private TestClass1()
    {
        _readonlyStringField1 = "some useless value";
        readonlyStringField2 = "some useless value";
        _nonReadonlyBooleanField = false;
    }

    public TestClass1(
        string arg1,
        string arg2)
    {
        // some comment
        _readonlyStringField1 = arg1;
        readonlyStringField2 = arg2;
        _nonReadonlyBooleanField = true;
    }

    public static TestClass1 SomePublicFactoryMethod()
    {
        return new TestClass1();
    }

    public async Task HandleOnSomeEventCallbackAsync(
        string variable1,
        string variable2,
        string variable3,
        string variable4)
    {
        var someVariableWithBadNamingConvention = "Hi";
        var someOtherVariableWithBadNamingConvention = "there";

        Console.WriteLine(
            $"{someVariableWithBadNamingConvention} {someOtherVariableWithBadNamingConvention} !"
        );

        _nonReadonlyBooleanField = true;

        if (_nonReadonlyBooleanField)
        {
            Console.WriteLine(
                $"{someVariableWithBadNamingConvention} {someOtherVariableWithBadNamingConvention} again !"
            );
        }
    }

    public async void OnSomeEventCallbackAsync(
        object sender,
        EventArgs e)
    {
#define VRAI

#if VRAI
        var variable1 = "1";
        var variable2 = "2";
        var variable3 = "3";
        var variable4 = "4";
#endif

        void Test(string parameter1, string parameter2, string parameter3, string parameter4, string parameter5)
        {
        }

#pragma warning disable 4014
        HandleOnSomeEventCallbackAsync(
            variable1: variable1,
            variable2: variable2,
            variable3: variable3,
            variable4: variable4
        );
#pragma warning restore 4014

        MapControllerRoute(a: "1", b: "2");
    }

    public Task SomeAsyncMethodAsync()
    {
        return Task.CompletedTask;
    }

    public Task SomeAsyncMethodWithSuffixAsync()
    {
        return Task.CompletedTask;
    }

    #region Private use

    private void MapControllerRoute(
        string a,
        string b)
    {
    }

    private void SomePrivateMethod()
    {
    }

    private static void SomePrivateStaticMethod()
    {
    }
}

public sealed class TestClass2 : TestClass1<int>
{
    public TestClass2(
        int parameter1,
        int parameter2)
    {
        var variable1 = Method1(
            Method1(
                "2222222222222222222222222222222",
                new DummyClass(
                    "DummyClassParameter1",
                    "DummyClassParameter2",
                    "DummyClassParameter3",
                    "DummyClassParameter4"
                ),
                "2222222222222222222222222222222",
                "2222222222222222222222222222222"
            ),
            new DummyClass(
                "DummyClassParameter1",
                "DummyClassParameter2",
                "DummyClassParameter3",
                "DummyClassParameter4"
            ),
            Method1(
                "3333333333333333333333333333333",
                new DummyClass(
                    "DummyClassParameter1",
                    "DummyClassParameter2",
                    "DummyClassParameter3",
                    "DummyClassParameter4"
                ),
                "3333333333333333333333333333333",
                "3333333333333333333333333333333"
            ),
            "1111111111111111111111111111111111111"
        );
    }

    private TestClass2(
        int parameter1,
        int parameter2)
    {
        var result = new List<GarantieDto>();
        result.Add(
            new GarantieDto
            {
                CotisationGarantie = cotisationObligatoireMensuelle,
                IsObligatoireGarantie = true,
                IsSouscriteGarantie = true,
                NomGarantie = $"Capital décès",
                CotisationType = CotisationTypeDto.Cotisation
            },
            "test1",
            "test2"
        );
        result.Add(
            new GarantieDto()
            {
                CotisationGarantie = cotisationObligatoireMensuelle,
                IsObligatoireGarantie = true,
                IsSouscriteGarantie = true,
                NomGarantie = $"Capital décès",
                CotisationType = CotisationTypeDto.Cotisation
            },
            "test1",
            "test2"
        );

        var variable1 = new[]
        {
            new SignatureInfoDto(
                SignatoryTypeDto.Fournisseur,
                new PositionPdfDto(4, 42, 130, 150, 55)
            ),
            new SignatureInfoDto(
                SignatoryTypeDto.TiersDeConfiance,
                new PositionPdfDto(4, 211, 105, 150, 80)
            ),
            new SignatureInfoDto(SignatoryTypeDto.Client, new PositionPdfDto(4, 380, 105, 140, 80))
        };

        var variable2 = new[]
        {
        };

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
            )
        };

        var variable4 = new[]
        {
            "1",
            "2",
            "3"
        };

        var variable5 = new List<string>
        {
            "item1",
            "item2",
            "item3",
            "item4",
            "item5",
            "item6",
            "item7",
            "item8",
            "item9",
            "item10"
        };

        var variable6 = Json(
            new
            {
                Parameter1 = "1",
                Parameter2 = "2",
                Parameter2 = "3",
                Parameter2 = "4",
                Parameter2 = "5"
            }
        );

        var variable7 = Json(
            new
            {
                Parameter1 = "1",
                Parameter2 = "2",
                Parameter2 = "3",
                Parameter2 = "4",
                Parameter2 = "5"
            }
        );

        var variable8 = Json(
            new()
            {
                Parameter1 = "1",
                Parameter2 = "2",
                Parameter2 = "3",
                Parameter2 = "4",
                Parameter2 = "5"
            }
        );

        var variable9 = Json(
            new()
            {
                Parameter1 = "1",
                Parameter2 = "2",
                Parameter2 = "3",
                Parameter2 = "4",
                Parameter2 = "5"
            }
        );
    }

    public string Method1(
        string parameter1,
        DummyClass parameter2,
        string parameter3,
        string parameter4)
    {
        return null;
    }

    public string Method2()
    {
        return Method1(
            Method1(
                "2222222222222222222222222222222",
                new DummyClass(
                    "DummyClassParameter1",
                    "DummyClassParameter2",
                    "DummyClassParameter3",
                    "DummyClassParameter4"
                ),
                "2222222222222222222222222222222",
                "2222222222222222222222222222222"
            ),
            new DummyClass(
                "DummyClassParameter1",
                "DummyClassParameter2",
                "DummyClassParameter3",
                "DummyClassParameter4"
            ),
            Method1(
                "3333333333333333333333333333333",
                new DummyClass(
                    "DummyClassParameter1",
                    "DummyClassParameter2",
                    "DummyClassParameter3",
                    "DummyClassParameter4"
                ),
                "3333333333333333333333333333333",
                "3333333333333333333333333333333"
            ),
            "1111111111111111111111111111111111111"
        );
    }
}

private record DummyClass(string parameter1, string parameter2, string parameter3, string parameter4);

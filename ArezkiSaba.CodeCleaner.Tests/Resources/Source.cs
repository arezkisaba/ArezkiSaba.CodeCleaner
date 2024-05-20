﻿using ArezkiSaba.CodeCleaner.Tests;
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

public class TestClass1<T>
{
    public TestClass1(string arg1, string arg2)
    {
        // some comment
        _readonlyStringField1 = arg1;
        readonlyStringField2 = arg2;
        nonReadonlyBooleanField = true;

    }

    private string _readonlyStringField1;



    public async void OnSomeEventCallback(
                    object sender,
            EventArgs e)
    {
#define VRAI

#if VRAI
        string variable1 = "1";
        string variable2 = "2";
        string variable3 = "3";
        string variable4 = "4";
#endif



#pragma warning disable 4014
        _handleOnSomeEventCallback(
                                        variable1: variable1,
            variable2: variable2,
            variable3: variable3, variable4: variable4);
#pragma warning restore 4014


        MapControllerRoute(
            a: "1",
            b: "2");
    }

    private void MapControllerRoute(string a, string b)
    {
    }

    private TestClass1()
    {
        _readonlyStringField1 = "some useless value";
        readonlyStringField2 = "some useless value";
        nonReadonlyBooleanField = false;
    }

    public Task SomeAsyncMethodWithSuffixAsync()
    {
        return Task.CompletedTask;
    }

    public bool CanDoSomethingElse
    {
        get { return canDoSomethingElse; }
        set { canDoSomethingElse = value; }
    }

    public event EventHandler someOtherEventCamelCasedTriggered;

    private void SomePrivateMethod()
    {
    }

    public bool CanDoSomething
    {
        get { return _canDoSomething; }
        set { _canDoSomething = value; }
    }

    public Task SomeAsyncMethod()
    {


        return Task.CompletedTask;

    }

    private static void SomePrivateStaticMethod()
    {


    }

    private bool canDoSomethingElse;
    public async Task _handleOnSomeEventCallback(string variable1,
                string variable2,
        string variable3, string variable4)
    {


        var _someVariableWithBadNamingConvention = "Hi";
        var SomeOtherVariableWithBadNamingConvention = "there";

        Console.WriteLine($"{_someVariableWithBadNamingConvention} {SomeOtherVariableWithBadNamingConvention} !");



        nonReadonlyBooleanField = true;


        if (nonReadonlyBooleanField)
        {


            Console.WriteLine($"{_someVariableWithBadNamingConvention} {SomeOtherVariableWithBadNamingConvention} again !");



        }




    }

    public bool CanDoSomeOtherThing { get; set; }

    private static string _someStaticField;

    public event EventHandler SomeEventTriggered;




    private bool nonReadonlyBooleanField;

    private const string someConstField = "SOME CONST VALUE";



    private bool _canDoSomething;

    #region SomeRegionName


    public static TestClass1 SomePublicFactoryMethod()
    {

        return new TestClass1();


    }


    #endregion
}

public class TestClass2 : TestClass1<int>
{
    private TestClass2(int variable1, int variable2) : base()
    {
        var result = new List<GarantieDto>();
        result.Add(new GarantieDto { CotisationGarantie = cotisationObligatoireMensuelle, IsObligatoireGarantie = true, IsSouscriteGarantie = true, NomGarantie = $"Capital décès", CotisationType = CotisationTypeDto.Cotisation, }, "test1", "test2");


        var position = new[]
        {
            new SignatureInfoDto(SignatoryTypeDto.Fournisseur, new PositionPdfDto(4, 42, 130, 150, 55)),
            new SignatureInfoDto(SignatoryTypeDto.TiersDeConfiance, new PositionPdfDto(4, 211, 105, 150, 80)),
            new SignatureInfoDto(SignatoryTypeDto.Client, new PositionPdfDto(4, 380, 105, 140, 80))
        };

        var array = new[]
        {
            new string(['a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a'], 0, 0),
            new string(['b', 'b', 'b', 'b', 'b', 'b', 'b', 'b', 'b', 'b', 'b', 'b', 'b', 'b', 'b', 'b', 'b', 'b', 'b', 'b', 'b'], 0, 0)
        };

        var items = new[]
        {
            "1",
            "2",
            "3"
        };
    }

    public TestClass2(int variable1, int variable2)
        : this(variable1, variable2)
    {
        var variable1 = Method1(Method1("2222222222222222222222222222222", new DummyClass("DummyClassParameter1", "DummyClassParameter2", "DummyClassParameter3", "DummyClassParameter4"), "2222222222222222222222222222222", "2222222222222222222222222222222"), new DummyClass("DummyClassParameter1", "DummyClassParameter2", "DummyClassParameter3", "DummyClassParameter4"), Method1("3333333333333333333333333333333", new DummyClass("DummyClassParameter1", "DummyClassParameter2", "DummyClassParameter3" "DummyClassParameter4"), "3333333333333333333333333333333", "3333333333333333333333333333333"), "1111111111111111111111111111111111111"
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
        return Method1(Method1("2222222222222222222222222222222", new DummyClass("DummyClassParameter1", "DummyClassParameter2", "DummyClassParameter3", "DummyClassParameter4"), "2222222222222222222222222222222", "2222222222222222222222222222222"), new DummyClass("DummyClassParameter1", "DummyClassParameter2", "DummyClassParameter3", "DummyClassParameter4"), Method1("3333333333333333333333333333333", new DummyClass("DummyClassParameter1", "DummyClassParameter2", "DummyClassParameter3" "DummyClassParameter4"), "3333333333333333333333333333333", "3333333333333333333333333333333"), "1111111111111111111111111111111111111");
    }
}

private record DummyClass(string parameter1, string parameter2, string parameter3, string parameter4);

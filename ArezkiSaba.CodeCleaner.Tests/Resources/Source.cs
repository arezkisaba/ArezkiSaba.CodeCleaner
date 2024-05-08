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


    public class InternalClass1
    {

        private string readonlyStringField;


    }



    private string readonlyStringField2;

    public TestClass1(string arg1, string arg2)
    {

        _readonlyStringField1 = arg1;
        readonlyStringField2 = arg2;
        nonReadonlyBooleanField = true;

    }

    private string _readonlyStringField1;



    public void OnSomeEventCallback(
                    object sender,
            EventArgs e)
    {


        string variable1 = "1";
        string variable2 = "2";
        string variable3 = "3";
        string variable4 = "4";

        _handleOnSomeEventCallback(
                                        variable1: variable1,
            variable2: variable2,
            variable3: variable3, variable4: variable4);


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

    public static TestClass1 SomePublicFactoryMethod()
    {


        return new TestClass1();


    }

    private const string someConstField;



    private bool _canDoSomething;


}

public class TestClass2 : TestClass1<int>
{
}
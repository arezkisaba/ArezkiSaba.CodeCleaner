﻿using System;
using System;
using ArezkiSaba.CodeCleaner.Tests;
using ArezkiSaba.CodeCleaner.Tests;


namespace ArezkiSaba.CodeCleaner.Tests;



public partial class TestClass1<T>
{



    public TestClass1()
    {


        _readonlyStringField1 = "some useless value";
        _readonlyStringField2 = "some useless value";
        nonReadonlyBooleanField = false;

    }

    
    private string _readonlyStringField2;

    public TestClass1(string arg1)
    {

        _readonlyStringField1 = arg1;
        _readonlyStringField2 = arg1;
        nonReadonlyBooleanField = false;

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
                                        variable1,
            variable2, variable3, variable4
        );


    }

    private TestClass1()
    {
    }

    public Task SomeAsyncMethodWithSuffixAsync()
    {
        return Task.CompletedTask;
    }

    public event EventHandler someOtherEventCamelCasedTriggered;

    private void SomePrivateMethod()
    {
    }

    public int CanDoSomething
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



    private int _canDoSomething;


}
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Drawing;

public interface IAnimal
{
    Task _eatAsync();
}

public static class Constants
{
}

public class DogEventArgs : EventArgs
{
    public string _dogName { get; set; }
}

public partial class Dog : IAnimal
{
    public event EventHandler<DogEventArgs> lunchFinished;

    public static Dog create(string _name)
    {
        return new Dog(_name);
    }

    private string _someUselessField;

    public string _name { get; private set; }



    private Dog(
        string NAME)
    {
        _name = NAME;
    }

    public struct DogNames
    {
        public const string Fido = "Fido";
        public static string Rex = "Rex";
    }

    public class AnotherClass1
    {
        public void SomeMethod()
        {

        }

        public AnotherClass1()
        {
        }

        private int _someField1;
    }
}

public partial class Dog
{


    private async Task SpeakAsync()
    {

        await Task.Run(() => Console.WriteLine($"{_name} says: Woof!"));

    }


    public async Task _eatAsync()
    {


        var random = new Random();
        int delay = random.Next(2000, 4000);

        Console.WriteLine($"{_name} starts eating...");
        await Task.Delay(delay);

        lunchFinished?.Invoke(null, new DogEventArgs { _dogName = _name });


        await SpeakAsync();


    }

}

public class AnimalList<T> where T : IAnimal
{


    public async Task MakeAllAnimalsEatAsync()
    {

        var eatTasks = animals.Select(animal => animal._eatAsync());
        await Task.WhenAll(eatTasks);

    }
    private List<T> animals = new();

    public void AddAnimal(
        T animal)
    {

        animals.Add(animal);

    }


}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public interface IAnimal
{
    Task EatAsync();
}

public static class Constants
{
}

public class DogEventArgs : EventArgs
{
    public string DogName { get; set; }
}

public partial class Dog : IAnimal
{
    public event EventHandler<DogEventArgs> LunchFinished;

    public static Dog Create(
        string name)
    {
        return new Dog(name);
    }

    private readonly string _someUselessField;

    public string Name { get; private set; }

    private Dog(
        string name)
    {
        Name = name;
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

        private string _someProperty1;
        public string SomeProperty1
        {
            get
            {
                return _someProperty1;
            }
            set => _someProperty1 = value;
        }

        public AnotherClass1()
        {
        }

        private readonly string _someProperty2;
        public string SomeProperty2
        {
            get
            {
                return _someProperty2;
            }
        }

        private readonly int _someField1;
    }
}

public partial class Dog
{
    private async Task SpeakAsync()
    {
        await Task.Run(() => Console.WriteLine($"{Name} says: Woof!"));
    }

    public async Task EatAsync()
    {
        var random = new Random();
        var delay = random.Next(2000, 4000);

        Console.WriteLine($"{Name} starts eating...");
        await Task.Delay(delay);

        LunchFinished?.Invoke(null, new DogEventArgs
            {
                DogName = Name
            });

        await SpeakAsync();
    }
}

public class AnimalList<T> where T : IAnimal
{
    public async Task MakeAllAnimalsEatAsync()
    {
        var eatTasks = _animals.Select(animal => animal.EatAsync());
        await Task.WhenAll(eatTasks);
    }
    private readonly List<T> _animals = new();

    public void AddAnimal(
        T animal)
    {
        _animals.Add(animal);
    }
}

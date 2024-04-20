using System.Threading.Tasks;
using System.Linq;
using System.Linq;
using System;
using System.Collections.Generic;

public interface IAnimal
{
    Task SpeakAsync();
    Task EatAsync();
}

public class DogEventArgs : EventArgs
{
    public string DogName { get; set; }
}

public partial class Dog : IAnimal
{
    public string Name { get; private set; }
    public event EventHandler<DogEventArgs> LunchFinished;

    private Dog(string name)
    {
        Name = name;
    }

    public static Dog Create(string name)
    {
        return new Dog(name);
    }
}

public partial class Dog
{
    public async Task SpeakAsync()
    {
        await Task.Run(() => Console.WriteLine($"{Name} says: Woof!"));
    }

    public async Task EatAsync()
    {
        var random = new Random();
        var delay = random.Next(2000, 4000);

        Console.WriteLine($"{Name} starts eating...");
        await Task.Delay(delay);

        LunchFinished?.Invoke(null, new DogEventArgs { DogName = Name });

        await SpeakAsync();
    }
}

public class AnimalList<T> where T : IAnimal
{
    private List<T> animals = new();

    public void AddAnimal(T animal)
    {
        animals.Add(animal);
    }

    public async Task MakeAllAnimalsEatAsync()
    {
        var eatTasks = animals.Select(animal => animal.EatAsync());
        await Task.WhenAll(eatTasks);
    }
}

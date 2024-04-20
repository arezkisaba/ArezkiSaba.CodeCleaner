using System;

var dog1 = Dog.create("Rex");
dog1.lunchFinished += (sender, e) => Console.WriteLine($"{e.DogName} has finished eating.");

var dog2 = Dog.create("Fido");
dog2.lunchFinished += (sender, e) => Console.WriteLine($"{e.DogName} has finished eating.");

var dogs = new AnimalList<Dog>();
dogs.AddAnimal(dog1);
dogs.AddAnimal(dog2);

await dogs.MakeAllAnimalsEatAsync();

Console.ReadLine();
using System;

var dog1 = Dog.Create(Dog.DogNames.Rex);
dog1.LunchFinished += (sender, e) => Console.WriteLine($"{e.DogName} has finished eating.");

var dog2 = Dog.Create(Dog.DogNames.Fido);
dog2.LunchFinished += (sender, e) => Console.WriteLine($"{e.DogName} has finished eating.");

var dogs = new AnimalList<Dog>();
dogs.AddAnimal(dog1);
dogs.AddAnimal(dog2);

await dogs.MakeAllAnimalsEatAsync();

Console.ReadLine();
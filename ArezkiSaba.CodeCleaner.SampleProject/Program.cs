using System;

var dog1 = Dog.create(Constants.DogNames.Rex);
dog1.lunchFinished += (sender, e) => Console.WriteLine($"{e._dogName} has finished eating.");

var dog2 = Dog.create(Constants.DogNames.Fido);
dog2.lunchFinished += (sender, e) => Console.WriteLine($"{e._dogName} has finished eating.");

var dogs = new AnimalList<Dog>();
dogs.AddAnimal(dog1);
dogs.AddAnimal(dog2);

await dogs.MakeAllAnimalsEatAsync();

Console.ReadLine();
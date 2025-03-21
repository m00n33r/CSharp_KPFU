using System;


abstract class Player
{
    public int Speed { get; set; }
    public int Health { get; set; }
    public string Name { get; set; }
    protected Player(string name) 
    {
        Speed = 10;
        Health = 100;
        Name = name;
        Console.WriteLine("Экземпляр Player создан.");
    }

    public abstract void hi();
}

class Man : Player
{
    public int Age { get; set; }
    private string _bio = "";
    public string Bio {
        get { return _bio; }
        private set {_bio = value;}
    }

    // Конструктор по умолчанию
    public Man() : base("defaultPlayer") {}

    // Конструктор с параметром
    public Man(string name) : base(name) {}

    public override void hi()
    {
        Console.WriteLine("Hi, I'm a man");
    }

    public void SetBio(string new_bio) {
        Bio = new_bio;
        Console.WriteLine("Your bio successfully updated");
    }

    public void PrintInfo() 
    {
        Console.WriteLine($"Name: {Name}, Age: {Age}, Bio: {Bio}");
    }

    private void Calculate() 
    {
        Console.WriteLine("Calculating something...");
    }

    public static Man CreateDefaultMan()
    {
        return new Man
        {
            Name = "DefaultMan",
            Age = 25,
            Bio = "Just a default man."
        };
    }
}

class Program
{
    public static void Main(string[] args)
    {
        Player player = new Man("Ignat");
    }
}
using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Collections.Generic;

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
    public abstract void Hi();
}

class Man : Player
{
    public int Age { get; set; }
    private string _bio = "";
    public string Bio
    {
        get { return _bio; }
        private set { _bio = value; }
    }

    public Man() : base("defaultPlayer") { }
    public Man(string name) : base(name) { }

    public override void Hi()
    {
        Console.WriteLine("Hi, I'm a man");
    }

    public void SetBio(string new_bio)
    {
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


public class DependencyContainer
{
    private readonly Dictionary<Type, object> _singletons = new Dictionary<Type, object>();
    private readonly Dictionary<Type, Func<object>> _transient = new Dictionary<Type, Func<object>>();
    private readonly Dictionary<Type, Func<object>> _scoped = new Dictionary<Type, Func<object>>();
    private readonly Dictionary<Type, object> _scopedInstances = new Dictionary<Type, object>();


    public void RegisterTransient<TInterface, TImplementation>() where TImplementation : TInterface
    {
        _transient[typeof(TInterface)] = () => Activator.CreateInstance(typeof(TImplementation));
    }
    public void RegisterSingleton<TInterface, TImplementation>() where TImplementation : TInterface
    {
        _singletons[typeof(TInterface)] = Activator.CreateInstance(typeof(TImplementation));
    }
    public void RegisterScoped<TInterface, TImplementation>() where TImplementation : TInterface
    {
        _scoped[typeof(TInterface)] = () =>
        {
            if (_scopedInstances.TryGetValue(typeof(TImplementation), out var instance)) return instance;
            var newInstance = Activator.CreateInstance(typeof(TImplementation));
            _scopedInstances[typeof(TImplementation)] = newInstance;
            return newInstance;
        };
    }


    public T Resolve<T>()
    {
        if (_singletons.TryGetValue(typeof(T), out var singletonInstance)) return (T)singletonInstance;
        if (_transient.TryGetValue(typeof(T), out var transientFactory)) return (T)transientFactory();
        if (_scoped.TryGetValue(typeof(T), out var scopedFactory)) return (T)scopedFactory();
        return (T)Activator.CreateInstance(typeof(T));

    }

}


class Program
{
    static void Main(string[] args)
    {
        Man man = new Man("John");
        man.SetBio("I am John Doe, a software engineer!");
        man.PrintInfo();


        GenerateHtmlDocumentation(typeof(Player), "Player.html");
        GenerateHtmlDocumentation(typeof(Man), "Man.html");

        // 2.a) Работа с конструкторами
        ConstructorInfo[] constructors = typeof(Man).GetConstructors();
        foreach (ConstructorInfo constructor in constructors)
        {
            Console.WriteLine($"Constructor: {constructor.Name}, Access: {constructor.IsPublic}");
            ParameterInfo[] parameters = constructor.GetParameters();
            foreach (ParameterInfo parameter in parameters)
            {
                Console.WriteLine($"\tParameter: {parameter.ParameterType} {parameter.Name}");
            }
            try
            {
                object manInstance;
                if(parameters.Length == 0) manInstance = constructor.Invoke(null);
                else manInstance = constructor.Invoke(new object[] { "Mike" });
                Console.WriteLine($"Instance created successfully. Name: {(manInstance as Man).Name}");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating instance: {ex.Message}");
            }

        }

        // 2.b) Работа с методами
        MethodInfo[] methods = typeof(Man).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        foreach(MethodInfo method in methods)
        {
            if(method.Name == "Calculate")
            {
                method.Invoke(man, null);
                break;
            }
        }


        //3. Контейнер внедрения зависимостей
        var container = new DependencyContainer();
        container.RegisterTransient<Player, Man>();
        container.RegisterSingleton<ILogger, ConsoleLogger>();
        container.RegisterScoped<IRepository, MyRepository>();


        var player1 = container.Resolve<Player>();
        var player2 = container.Resolve<Player>();
        var logger = container.Resolve<ILogger>();
        var repo1 = container.Resolve<IRepository>();
        var repo2 = container.Resolve<IRepository>();


        Console.WriteLine($"Player1 == Player2: {player1 == player2}"); //Transient 
        Console.WriteLine($"Logger1 == Logger2: {logger == container.Resolve<ILogger>()}"); //Singleton
        Console.WriteLine($"Repo1 == Repo2: {repo1 == repo2}"); //Scoped

    }


    static void GenerateHtmlDocumentation(Type type, string fileName)
    {
        string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "docs");
        Directory.CreateDirectory(directoryPath);
        string filePath = Path.Combine(directoryPath, fileName);

        using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
        {
            writer.WriteLine("<!DOCTYPE html>");
            writer.WriteLine("<html>");
            writer.WriteLine("<head>");
            writer.WriteLine($"<title>Документация для класса {type.Name}</title>");
            writer.WriteLine("</head>");
            writer.WriteLine("<body>");
            writer.WriteLine($"<h1>Класс: {type.Name}</h1>");
            writer.WriteLine("<hr>");

            // Поля
            writer.WriteLine("<h2>Поля:</h2>");
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                writer.WriteLine($"<p>{field.FieldType} {field.Name} ({(field.IsPrivate ? "private" : "public")})</p>");
            }

            // Свойства
            writer.WriteLine("<h2>Свойства:</h2>");
            foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                writer.WriteLine($"<p>{prop.PropertyType} {prop.Name} ({(prop.GetMethod?.IsPublic == true ? "public" : "private")})</p>");
            }

            // Методы
            writer.WriteLine("<h2>Методы:</h2>");
            foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                if (!method.IsSpecialName)
                {
                    writer.WriteLine($"<p>{method.ReturnType} {method.Name} ({(method.IsPublic ? "public" : "private")})</p>");
                }
            }

            // Статические методы
            writer.WriteLine("<h2>Статические методы:</h2>");
            foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
            {
                writer.WriteLine($"<p>{method.ReturnType} {method.Name} ({(method.IsPublic ? "public" : "private")})</p>");
            }

            writer.WriteLine("</body>");
            writer.WriteLine("</html>");
        }
    }
}


// Интерфейсы для DI
interface ILogger
{
    void Log(string message);
}

interface IRepository
{
    void Save();
}

class ConsoleLogger : ILogger
{
    public void Log(string message)
    {
        Console.WriteLine($"Console Log: {message}");
    }
}

class MyRepository : IRepository
{
    public void Save()
    {
        Console.WriteLine("Data saved to repository.");
    }
}

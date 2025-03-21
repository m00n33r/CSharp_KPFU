using System;


class Player
{
    private static Player player;

    private Player() 
    {
        Console.WriteLine("Экземпляр Player создан.");
    }

    public static Player create_player()
    {
        if(player == null)
        {
            player = new Player();
        }
            return player;
    }
}

class Program
{
    public static void Main(string[] args)
    {
        Player player = Player.create_player();
    }
}
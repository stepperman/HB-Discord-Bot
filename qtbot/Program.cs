using System;

class Program
{
    static void Main(string[] args)
       => new qtbot.Bot().StartAsync().GetAwaiter().GetResult();
}
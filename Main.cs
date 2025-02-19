using System;

namespace ValeraJesus;

class Program {
    public static void Main() {
        var core = new Core();
        core.Work();
        Console.WriteLine("Бот запущен. Для выхода жмякните клавишу \"Ввод\"");
        Console.ReadLine();
    }
}
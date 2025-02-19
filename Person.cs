using System.Collections.Generic;

namespace ValeraJesus;

class Person {
    long host_id; //id наблюдаемого (с ОВЗ)
    string username; //Его имя
    List<long> trusted_ids; //Список id доверенных

    public long PersonID {
        get => host_id; //Получение id наблюдаемого 
    }

    public long[] TrustedPersonsIDs {
        get => trusted_ids.ToArray(); //Получение набора id доверенных лиц как массива
    }

    public void AddTrustedPerson(long id)
        => trusted_ids.Add(id); //Добавление доверенного

    public string Name {
        get => username; //Получение имени
    }

    public Person(long id, string name) { //Конструктор класса
        host_id = id;
        username = name;
        trusted_ids = new();
    }
}
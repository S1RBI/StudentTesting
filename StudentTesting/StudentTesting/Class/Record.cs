using System.Collections.Generic;

public class Record
{
    public int id { get; set; }
    public string order { get; set; }
    public string mail { get; set; }
    public string surname { get; set; }
    public string name { get; set; }
    public string patronymic { get; set; }
}

public class RecordsResponse
{
    public int Count { get; set; }
    public List<Record> Results { get; set; }
}
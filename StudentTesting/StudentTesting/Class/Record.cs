using System.Collections.Generic;

internal class Record
{
    public int id { get; set; }
    public string order { get; set; }
    public string mail { get; set; }
    public string surname { get; set; }
    public string name { get; set; }
    public string patronymic { get; set; }
    public string group { get; set; }
}

internal class RecordsResponse
{
    public string count { get; set; }
    public string next { get; set; }
    public string previous { get; set; }
    public List<Record> Results { get; set; }
}
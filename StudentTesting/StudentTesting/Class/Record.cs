using System;
using System.Collections.Generic;
using Newtonsoft.Json;

internal class TestStudent
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("order")]
    public decimal Order { get; set; }

    [JsonProperty("dateTime")]
    public DateTime DateTime { get; set; }

    [JsonProperty("student")]
    public List<StructJson> Student { get; set; }

    [JsonProperty("ball")]
    public int? Ball { get; set; }

    [JsonProperty("check")]
    public bool Check { get; set; }

    [JsonProperty("test")]
    public List<StructJson> Test { get; set; }
}
//Класс для списка наследуемых значений
internal class StructJson
{
    [JsonProperty("id")]
    public int Id { get; set; } //содержит значение id наследуемой таблицы по которому можно найти запись в основной таблице

    [JsonProperty("value")]
    public string Value { get; set; } //содержит значение первого столбца после id
}

public class Student
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("order")]
    public decimal Order { get; set; }

    [JsonProperty("mail")]
    public string Mail { get; set; }

    [JsonProperty("surname")]
    public string Surname { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("patronymic")]
    public string Patronymic { get; set; }

    [JsonProperty("group")]
    public string Group { get; set; }
}
//Класс для хранения списка записей таблицы
internal class Response<T>
{
    public string count { get; set; }
    public string next { get; set; }
    public string previous { get; set; }
    public List<T> Results { get; set; }
}
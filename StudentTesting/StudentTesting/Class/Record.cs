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

internal class StructJson
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("value")]
    public string Value { get; set; }
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

internal class Response<T>
{
    public string count { get; set; }
    public string next { get; set; }
    public string previous { get; set; }
    public List<T> Results { get; set; }
}
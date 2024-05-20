using System;
using System.Collections.Generic;
using Newtonsoft.Json;


internal class Question
{
    [JsonProperty("question")]
    public string QuestionText { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("choices")]
    public List<string> Choices { get; set; }

    [JsonProperty("correctAnswers")]
    public List<string> CorrectAnswers { get; set; }

    [JsonProperty("points")]
    public double Points { get; set; }

    public List<string> Answer { get; set; } = new List<string>();
}

internal class FileMetadata
{
    [JsonProperty("url")]
    public string Url { get; set; }

    [JsonProperty("thumbnails")]
    public object Thumbnails { get; set; }

    [JsonProperty("visible_name")]
    public string VisibleName { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("size")]
    public int Size { get; set; }

    [JsonProperty("mime_type")]
    public string MimeType { get; set; }

    [JsonProperty("is_image")]
    public bool IsImage { get; set; }

    [JsonProperty("image_width")]
    public object ImageWidth { get; set; }

    [JsonProperty("image_height")]
    public object ImageHeight { get; set; }

    [JsonProperty("uploaded_at")]
    public DateTime UploadedAt { get; set; }
}
internal class Test
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("order")]
    public decimal Order { get; set; }

    [JsonProperty("nameTest")]
    public string NameTest { get; set; }

    [JsonProperty("subject")]
    public List<StructJson> Subject { get; set; }

    [JsonProperty("fileJSON")]
    public List<FileMetadata> Files { get; set; }

    [JsonProperty("quantity")]
    public int Quantity { get; set; }

    [JsonProperty("period")]
    public int Period { get; set; }

    [JsonProperty("questions")]
    public List<Question> Questions { get; set; }
}

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
    public double Ball { get; set; }

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

internal class Student
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


internal class StructJsonComparer : IEqualityComparer<StructJson>
{
    public bool Equals(StructJson x, StructJson y)
    {
        return x.Value == y.Value && x.Id == y.Id;
    }

    public int GetHashCode(StructJson obj)
    {
        return obj.Value.GetHashCode() ^ obj.Id.GetHashCode();
    }
}
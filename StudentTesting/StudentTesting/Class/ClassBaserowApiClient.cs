using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;

internal class ClassBaserowApiClient
{
    private readonly HttpClient _httpClient;
    internal ClassBaserowApiClient()
    {
        // Создаем новый экземпляр HttpClient с указанием базового адреса,
        // полученного из конфигурационного файла.
        _httpClient = new HttpClient { BaseAddress = new Uri(ConfigurationManager.AppSettings["BaserowBaseUrl"]) };

        // Устанавливаем заголовок авторизации для HTTP-запросов, используя токен, 
        // полученный из конфигурационного файла.
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", ConfigurationManager.AppSettings["BaserowToken"]);
    }

    //Метод для получения записей
    private async Task<string> GetRecordsAsync(string tableId)
    {
        // Выполняем асинхронный GET-запрос к серверу, используя HttpClient.
        // Запрашиваем строки из конкретной таблицы Baserow по её идентификатору (tableId).
        // Параметр `user_field_names=true` указывает на то, что имена полей в ответе
        // должны быть как установлено пользователем, а не их технические идентификаторы.
        var response = await _httpClient.GetAsync($"database/rows/table/{tableId}/?user_field_names=true");

        // После получения ответа от сервера читаем содержимое тела ответа асинхронно как строку.
        return await response.Content.ReadAsStringAsync();

    }
    private async Task<string> GetRowRecordsAsync(string tableId, string idObject)
    {
        // Выполняем асинхронный GET-запрос к серверу, используя HttpClient.
        // Запрашиваем строки из конкретной таблицы Baserow по её идентификатору (tableId).
        // Параметр `user_field_names=true` указывает на то, что имена полей в ответе
        // должны быть как установлено пользователем, а не их технические идентификаторы.
        var response = await _httpClient.GetAsync($"database/rows/table/{tableId}/{idObject}/?user_field_names=true");

        // После получения ответа от сервера читаем содержимое тела ответа асинхронно как строку.
        return await response.Content.ReadAsStringAsync();

    }
    private async Task<string> SearchRecordsAsync(string tableId, string search, string column)
    {
        var response = await _httpClient.GetAsync($"database/rows/table/{tableId}/?user_field_names=true&search={Uri.EscapeDataString(column + ":" + search)}");

        // После получения ответа от сервера читаем содержимое тела ответа асинхронно как строку.
        return await response.Content.ReadAsStringAsync();

    }

    private async Task<string> GetIncludeExcludeRecordsAsync(string tableId, string excludeParameter, string includeExclude)
    {
        // Выполняем асинхронный GET-запрос к серверу, используя HttpClient.
        // Запрашиваем строки из конкретной таблицы Baserow по её идентификатору (tableId).
        // Параметр `user_field_names=true` указывает на то, что имена полей в ответе
        // должны быть как установлено пользователем, а не их технические идентификаторы.
        var response = await _httpClient.GetAsync($"database/rows/table/{tableId}/?user_field_names=true&{includeExclude}={Uri.EscapeDataString(excludeParameter)}");

        // После получения ответа от сервера читаем содержимое тела ответа асинхронно как строку.
        return await response.Content.ReadAsStringAsync();

    }

    // Функция для выполнения GET-запроса к таблице с фильтром
    private async Task<string> GetRecordsWithFilterAsync(string tableId, string filter)
    {
        var response = await _httpClient.GetAsync($"database/rows/table/{tableId}/?user_field_names=true&filters={filter}");
        return await response.Content.ReadAsStringAsync();
    }

    private async Task<List<Question>> LoadQuestionsFromFile(string fileUrl)
    {
        using (var httpClient = new HttpClient())
        {
            var response = await httpClient.GetAsync(fileUrl);
            response.EnsureSuccessStatusCode();

            var fileContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Question>>(fileContent);
        }
    }

    // Метод для получения информации о студенте по его адресу электронной почты.
    // Возвращает объект Student или null, если студент не найден.
    internal async Task<Student> GetStudentByEmailAsync(string email)
    {
        // Получаем данные из таблицы с указанным идентификатором: var recordsJson = await GetRecordsAsync(tableId);
        // Десериализуем JSON-строку в объект Response<Student>: var response = JsonConvert.DeserializeObject<Response<Student>>(recordsJson);
        // Ищем первую запись с указанным адресом электронной почты и возвращаем её.
        // Если такой студент не найден, возвращаем null.
        return JsonConvert.DeserializeObject<Response<Student>>(await SearchRecordsAsync(ConfigurationManager.AppSettings["Student"], email, "mail")).Results.FirstOrDefault();
        //return JsonConvert.DeserializeObject<Response<Student>>(await GetRecordsAsync(ConfigurationManager.AppSettings["Student"])).Results.FirstOrDefault(r => r.Mail == email);
    }

    // Метод для получения информации о тестах, связанных с определенным студентом по его идентификатору.
    // Возвращает список объектов TestStudent или пустой список, если тесты не найдены.
    internal async Task<List<TestStudent>> GetTestStudentByIdAsync(int id)
    {
        // Получаем данные из таблицы с указанным идентификатором: var recordsJson = await GetRecordsAsync(tableId);

        // Десериализуем JSON-строку в объект Response<TestStudent>: var response = JsonConvert.DeserializeObject<Response<TestStudent>>(recordsJson);

        // Фильтруем результаты, оставляя только тесты, связанные с студентом по его идентификатору.
        //var testsForStudent = response.Results
        //                               .Where(r => r.Student.Any(student => student.Id == id))
        //                               .ToList();

        return JsonConvert.DeserializeObject<Response<TestStudent>>(await GetRecordsAsync(ConfigurationManager.AppSettings["TestStudent"])).Results
                                   .Where(r => r.Student.Any(student => student.Id == id))
                                   .ToList();
    }

    //Получаем список Subject используя список TestStudent
    internal async Task<List<StructJson>> GetTestSubjectsByIdAsync(List<StructJson> lisTestStudent)
    {
        try
        {
            string[] excludedColumns = new string[] { "fileJSON" };
            string excludeParameter = string.Join(",", excludedColumns);
            var response = await GetIncludeExcludeRecordsAsync(ConfigurationManager.AppSettings["Test"], excludeParameter, "exclude");

            return JsonConvert.DeserializeObject<Response<Test>>(response)?.Results
                .Where(test => lisTestStudent.Any(student => student.Id == test.Id))
                .SelectMany(t => t.Subject)
                .ToList() ?? new List<StructJson>();
        }
        catch (Exception ex)
        {
            // Обработка исключения
            Console.WriteLine($"Ошибка: {ex.Message}");
            return new List<StructJson>();
        }
    }


    // Функция для десериализации ответа от сервера
    internal List<T> DeserializeResponse<T>(string responseContent)
    {
        var response = JsonConvert.DeserializeObject<Response<T>>(responseContent);
        return response.Results;
    }

    internal async Task<List<Question>> GetTestByIDAsync(string id)
    {
        try
        {
            var response = await GetRowRecordsAsync(ConfigurationManager.AppSettings["Test"], id);
            var deserializedResponse = JsonConvert.DeserializeObject<Test>(response);
            return deserializedResponse.Questions = await LoadQuestionsFromFile(deserializedResponse.Files[0].Url);
        }
        catch (Exception ex)
        {
            // Обработка исключения
            Console.WriteLine($"Ошибка: {ex.Message}");
            return null;
        }

    }

    // Основной метод для получения списка тестов
    internal async Task<List<Test>> GetTestsByStudentIdAndSubjectIdAsync(int studentId, int subjectId)
    {
        string studentFilter = $"{{\"filter_type\": \"AND\", \"filters\": [{{\"field\": \"student\", \"type\": \"link_row_has\", \"value\": \"{studentId}\"}}]}}";
        string studentResponse = await GetRecordsWithFilterAsync(ConfigurationManager.AppSettings["TestStudent"], Uri.EscapeDataString(studentFilter));

        string[] excludedColumns = new string[] { "fileJSON" };
        string excludeParameter = string.Join(",", excludedColumns);
        string subjectFilter = $"{{\"filter_type\": \"AND\", \"filters\": [{{\"field\": \"subject\", \"type\": \"link_row_has\", \"value\": \"{subjectId}\"}}]}}";
        string subjectResponse = await GetRecordsWithFilterAsync(ConfigurationManager.AppSettings["Test"], Uri.EscapeDataString(subjectFilter) + "&exclude=" + Uri.EscapeDataString(excludeParameter));
        
        var studentTests = DeserializeResponse<TestStudent>(studentResponse);
        var subjectTests = DeserializeResponse<Test>(subjectResponse);

        var filteredSubjectTests = subjectTests.Where(s => studentTests.Any(st => st.Id == s.Id)).ToList();

        return filteredSubjectTests;
    }


    // Метод для обновления записи
    internal async Task<bool> UpdateRecordAsync(string tableId, object updateObject, int idObject)
    {
        // Создаем контент запроса, сериализуя объект updateObject в формат JSON.
        var content = new StringContent(JsonConvert.SerializeObject(updateObject), System.Text.Encoding.UTF8, "application/json");

        // Создаем HTTP-запрос методом PATCH для обновления данных в определенной строке таблицы.
        var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"database/rows/table/{tableId}/{idObject}/?user_field_names=true")
        {
            Content = content // Устанавливаем контент запроса.
        };

        // Отправляем асинхронный запрос на сервер с созданным запросом.
        var response = await _httpClient.SendAsync(request);

        // Возвращаем true, если запрос завершился успешно (статус код 2xx), иначе возвращаем false.
        return response.IsSuccessStatusCode;
    }

    //пример
    internal async Task<bool> UpdateStudentByEmailAsync(string tableId, string email, Student updatedRecord)
    {
        // Находим запись по email
        var existingRecord = await GetStudentByEmailAsync(email);
        if (existingRecord == null) return false; // Запись с таким email не найдена

        // Обновляем данные в найденной записи
        existingRecord.Name = updatedRecord.Name;
        existingRecord.Surname = updatedRecord.Surname;

        if (await UpdateRecordAsync(tableId, existingRecord, existingRecord.Id))
            return true;
        else
            return false;
    }
}

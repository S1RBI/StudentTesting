﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    private async Task<string> SearchRecordsAsync(string tableId, string search)
    {
        var response = await _httpClient.GetAsync($"database/rows/table/{tableId}/?user_field_names=true&search={ search}");

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

    internal async Task<List<Question>> LoadQuestionsFromFile(string fileUrl)
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
        return JsonConvert.DeserializeObject<Response<Student>>(await SearchRecordsAsync(ConfigurationManager.AppSettings["Student"], email)).Results.FirstOrDefault();
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

    internal async Task<Test> GetTestByIDAsync(string id)
    {
        try
        {
            var response = await GetRowRecordsAsync(ConfigurationManager.AppSettings["Test"], id);
            var deserializedResponse = JsonConvert.DeserializeObject<Test>(response);
            return deserializedResponse;
        }
        catch (Exception ex)
        {
            // Обработка исключения
            Console.WriteLine($"Ошибка: {ex.Message}");
            return null;
        }

    }

    internal async Task<TestStudent> GetTestStudentByIDAsync(string idStudent, string idTest)
    {
        try
        {
            string studentFilter = $"{{\"filter_type\": \"AND\", \"filters\": [{{\"field\": \"student\", \"type\": \"link_row_has\", \"value\": \"{ idStudent}\"}}, {{\"field\": \"test\", \"type\": \"link_row_has\", \"value\": \"{ idTest}\"}}]}}";
            //string studentFilter = $"{{\"filter_type\": \"AND\", \"filters\": [{{\"field\": \"student\", \"type\": \"link_row_has\", \"value\": \"{idStudent}\"}}]}}";
            string studentResponse = await GetRecordsWithFilterAsync(ConfigurationManager.AppSettings["TestStudent"], Uri.EscapeDataString(studentFilter));

            var deserializedResponse = DeserializeResponse<TestStudent>(studentResponse).FirstOrDefault();
            return deserializedResponse;
        }
        catch (Exception ex)
        {
            // Обработка исключения
            Console.WriteLine($"Ошибка: {ex.Message}");
            return null;
        }

    }


    // Основной метод для получения списка тестов
    internal async Task<List<TestStudent>> GetTestsByStudentIdAndSubjectIdAsync(int studentId, int subjectId)
    {
        string studentFilter = $"{{\"filter_type\": \"AND\", \"filters\": [{{\"field\": \"student\", \"type\": \"link_row_has\", \"value\": \"{studentId}\"}}]}}";
        string studentResponse = await GetRecordsWithFilterAsync(ConfigurationManager.AppSettings["TestStudent"], Uri.EscapeDataString(studentFilter));

        string[] excludedColumns = new string[] { "fileJSON" };
        string excludeParameter = string.Join(",", excludedColumns);
        string subjectFilter = $"{{\"filter_type\": \"AND\", \"filters\": [{{\"field\": \"subject\", \"type\": \"link_row_has\", \"value\": \"{subjectId}\"}}]}}";
        string subjectResponse = await GetRecordsWithFilterAsync(ConfigurationManager.AppSettings["Test"], Uri.EscapeDataString(subjectFilter) + "&exclude=" + Uri.EscapeDataString(excludeParameter));
        
        var studentTests = DeserializeResponse<TestStudent>(studentResponse);
        var subjectTests = DeserializeResponse<Test>(subjectResponse);

        var filteredSubjectTests = studentTests.Where(s => subjectTests.Any(st => st.Id == s.Id)).ToList();

        return filteredSubjectTests;
    }


    // Метод для обновления записи
    internal async Task<bool> UpdateRecordAsync(string tableId, object updateObject, int idObject)
    {
        try
        {
            // Преобразуем updateObject в JObject для изменения структуры
            var jsonObject = JObject.FromObject(updateObject);

            // Преобразование полей
            if (!TryTransformFields(jsonObject, out string errorMessage))
            {
                Console.WriteLine(errorMessage);
                return false;
            }

            // Сериализация объекта в JSON
            var jsonString = JsonConvert.SerializeObject(jsonObject);
            var content = new StringContent(jsonString, System.Text.Encoding.UTF8, "application/json");

            // Формирование URL
            var requestUrl = $"database/rows/table/{tableId}/{idObject}/?user_field_names=true";

            // Создание запроса
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUrl)
            {
                Content = content
            };

            // Отправка запроса
            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                // Логирование ошибки
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Ошибка: {response.StatusCode}, Содержание: {responseContent}");
                return false;
            }
        }
        catch (Exception ex)
        {
            // Логирование исключения
            Console.WriteLine($"Исключение: {ex.Message}");
            return false;
        }
    }

    private bool TryTransformFields(JObject jsonObject, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (jsonObject["student"] is JArray studentArray && studentArray.Count > 0)
        {
            jsonObject["student"] = studentArray[0]["value"].ToString();
        }
        else
        {
            errorMessage = "Поле 'student' должно содержать допустимое значение.";
            return false;
        }

        if (jsonObject["test"] is JArray testArray && testArray.Count > 0)
        {
            jsonObject["test"] = testArray[0]["value"].ToString();
        }
        else
        {
            errorMessage = "Поле 'test' должно содержать допустимое значение.";
            return false;
        }

        return true;
    }

    //пример
    internal async Task<bool> UpdateStudentByIDAsync(string IdStudent, string idTest, double updatedRecord)
    {
        // Находим запись по email
        var deserializedResponse = await GetTestStudentByIDAsync(IdStudent, idTest);
        if (deserializedResponse == null) return false;

        // Обновляем данные в найденной записи
        deserializedResponse.Ball = updatedRecord;
        deserializedResponse.Check = true;

        return await UpdateRecordAsync(ConfigurationManager.AppSettings["TestStudent"], deserializedResponse, deserializedResponse.Id);
    }
}

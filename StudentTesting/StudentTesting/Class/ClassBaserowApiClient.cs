using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

internal class ClassBaserowApiClient
{
    private readonly HttpClient _httpClient;
    private ClassBaserowApiClient()
    {
        _httpClient = new HttpClient { BaseAddress = new Uri(ConfigurationManager.AppSettings["BaserowBaseUrl"]) };
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", ConfigurationManager.AppSettings["BaserowToken"]);
    }

    //Метод для получения записей
    private async Task<string> GetRecordsAsync(string tableId)
    {
        var response = await _httpClient.GetAsync($"database/rows/table/{tableId}/?user_field_names=true");
        return await response.Content.ReadAsStringAsync();
    }

    internal async Task<Student> GetStudentByEmailAsync(string tableId, string email)
    {
        return JsonConvert.DeserializeObject<Response<Student>>(await GetRecordsAsync(tableId)).Results.FirstOrDefault(r => r.Mail == email);
    }
    internal async Task<List<TestStudent>> GetTestStudentByIdAsync(string tableId, int id)
    {
        return JsonConvert.DeserializeObject<Response<TestStudent>>(await GetRecordsAsync(tableId)).Results
                                   .Where(r => r.Student.Any(student => student.Id == id))
                                   .ToList();
    }

    // Метод для обновления записи
    internal async Task<bool> UpdateRecordAsync(string tableId, object updateObject, int idObject)
    {
        var content = new StringContent(JsonConvert.SerializeObject(updateObject), System.Text.Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"database/rows/table/{tableId}/{idObject}/?user_field_names=true")
        {
            Content = content
        };
        var response = await _httpClient.SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    //пример
    internal async Task<bool> UpdateStudebtByEmailAsync(string tableId, string email, Student updatedRecord)
    {
        // Находим запись по email
        var existingRecord = await GetStudentByEmailAsync(tableId, email);
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

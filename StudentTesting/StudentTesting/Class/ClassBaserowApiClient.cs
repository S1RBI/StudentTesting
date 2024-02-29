using System;
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
    internal ClassBaserowApiClient()
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

    // Метод для обновления записи
    private async Task<bool> UpdateRecordAsync(string tableId, object updateObject, int idObject)
    {
        var content = new StringContent(JsonConvert.SerializeObject(updateObject), System.Text.Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"database/rows/table/{tableId}/{idObject}/?user_field_names=true")
        {
            Content = content
        };
        var response = await _httpClient.SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    internal async Task<Record> GetRecordByEmailAsync(string tableId, string email)
    {
        return JsonConvert.DeserializeObject<RecordsResponse>(await GetRecordsAsync(tableId)).Results.FirstOrDefault(r => r.mail == email);
    }

    //пример
    internal async Task<bool> UpdateRecordByEmailAsync(string tableId, string email, Record updatedRecord)
    {
        // Находим запись по email
        var existingRecord = await GetRecordByEmailAsync(tableId, email);
        if (existingRecord == null) return false; // Запись с таким email не найдена

        // Обновляем данные в найденной записи
        existingRecord.name = updatedRecord.name;
        existingRecord.surname = updatedRecord.surname;

        if (await UpdateRecordAsync(tableId, existingRecord, existingRecord.id))  
            return true;
        else
            return false;     
    }
}

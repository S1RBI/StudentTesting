using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class ClassBaserowApiClient
{
    private readonly HttpClient _httpClient;

    public ClassBaserowApiClient()
    {
        var baseUrl = ConfigurationManager.AppSettings["BaserowBaseUrl"];
        var token = ConfigurationManager.AppSettings["BaserowToken"];

        if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(token))
            throw new InvalidOperationException("Baserow base URL or token is not configured properly.");

        _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", token);
    }

    // Метод для получения записей
    public async Task<string> GetRecordsAsync(string tableId)
    {
        var response = await _httpClient.GetAsync($"database/rows/table/{tableId}/?user_field_names=true");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return content; // Возвращаем JSON ответ как строку
    }

    // Метод для обновления записи
    public async Task<bool> UpdateRecordAsync(string tableId, string recordId, object updateObject)
    {
        try
        {
            var json = JsonConvert.SerializeObject(updateObject);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"database/rows/table/{tableId}/{recordId}/")
            {
                Content = content
            };

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<Record> GetRecordByEmailAsync(string tableId, string email)
    {
        try
        {
            var uri = $"database/rows/table/{tableId}/?user_field_names=true";

            // Отправляем GET-запрос
            var response = await _httpClient.GetAsync(uri);
            response.EnsureSuccessStatusCode();

            // Читаем ответ в строку
            var content = await response.Content.ReadAsStringAsync();

            // Десериализуем JSON в объект RecordsResponse
            var recordsResponse = JsonConvert.DeserializeObject<RecordsResponse>(content);

            // Ищем запись с нужным email
            var record = recordsResponse.Results.FirstOrDefault(r => r.mail == email);

            return record;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> UpdateRecordByEmailAsync(string tableId, string email, Record updatedRecord)
    {
        try
        {
            // Находим запись по email
            var existingRecord = await GetRecordByEmailAsync(tableId, email);

            if (existingRecord == null)
            {
                // Запись с таким email не найдена
                return false;
            }

            // Обновляем данные в найденной записи
            existingRecord.name = updatedRecord.name;
            existingRecord.surname = updatedRecord.surname;

            // Сериализуем объект с обновленными данными в формат JSON
            var json = JsonConvert.SerializeObject(existingRecord);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"database/rows/table/{tableId}/{existingRecord.id}/?user_field_names=true")
            {
                Content = content
            };

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                // Успешно обновлено
                return true;
            }
            else
            {
                // Выведем информацию об ошибке в консоль
                Console.WriteLine($"Error updating record. StatusCode: {response.StatusCode}");
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error content: {errorContent}");

                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception during record update: {ex.Message}");
            return false;
        }
    }
}

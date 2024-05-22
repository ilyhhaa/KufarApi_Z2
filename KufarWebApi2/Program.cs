using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using KufarWebApi2;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Xunit;


public class Program
{
    public static async Task Main(string[] args)
    {
        
        string url = "https://api.kufar.by/search-api/v2/search/rendered-paginated?cat=1010&cur=USD&gtsy=country-belarus~province-minsk~locality-minsk&lang=ru&size=30&typ=sell";

        // Получение данных
        List<JObject> apartments = await GetApartmentsFromApi(url);

        
        List<Point> ourField = new List<Point> //Условная фигура поиска
        {
            new Point(27.536975, 53.868732),
            new Point(27.574, 53.898),
            new Point(27.574, 53.914),
            new Point(27.546, 53.914)
        };

        // Поиск квартир внутри фигуры
        var result = FindApartmentsInField(apartments, ourField);

        // Вывод найденных квартир
        Console.WriteLine("Квартиры внутри заданной фигуры:");
        foreach (var apartment in result)
        {
            Console.WriteLine(apartment);
        }
    }

    // Поиск
    public static List<JObject> FindApartmentsInField(List<JObject> apartments, List<Point> field)
    {
        if (field == null || field.Count < 3)
            throw new ArgumentException("Недопустимая фигура: должно быть минимум три точки.");

        List<JObject> apartmentsInField = new List<JObject>();

        foreach (var apartment in apartments)
        {
            var coordinatesToken = apartment["ad_parameters"]?.FirstOrDefault(attr => attr["p"]?.ToString() == "coordinates")?["v"] as JArray;
            if (coordinatesToken != null && coordinatesToken.Count == 2)
            {
                double longitude = coordinatesToken[0].Value<double>();
                double latitude = coordinatesToken[1].Value<double>();
                Point point = new Point(longitude, latitude);

                if (IsPointInField(point, field))
                {
                    apartmentsInField.Add(apartment);
                }
            }
        }

        return apartmentsInField;
    }

    // Проверяем находится ли точка внутри фигуры
    public static bool IsPointInField(Point point, List<Point> polygon)
    {
        int polygonLength = polygon.Count, i = 0;
        bool inside = false;
        double pointX = point.X, pointY = point.Y;
        double startX, startY, endX, endY;
        Point endPoint = polygon[polygonLength - 1];
        endX = endPoint.X;
        endY = endPoint.Y;
        while (i < polygonLength)
        {
            startX = endX;
            startY = endY;
            endPoint = polygon[i++];
            endX = endPoint.X;
            endY = endPoint.Y;
            inside ^= (endY > pointY) ^ (startY > pointY) && (pointX < (startX - endX) * (pointY - endY) / (startY - endY) + endX);
        }
        return inside;
    }

    //Берем данные из API
    public static async Task<List<JObject>> GetApartmentsFromApi(string url)
    {
        using HttpClient client = new HttpClient();
        HttpResponseMessage response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        string jsonResponse = await response.Content.ReadAsStringAsync();

        JObject data = JObject.Parse(jsonResponse);
        return data["ads"].ToObject<List<JObject>>();
    }
}


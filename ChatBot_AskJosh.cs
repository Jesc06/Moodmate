using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;

namespace Escarez_Finals_Mental_Health_Tracker
{
    public class ChatBot_AskJosh
    {



        private static readonly HttpClient httpClient = new HttpClient();
        private string apiKey = "";
        private string apiUrl = "";

        public async Task<string> GetDeepSeekResponse(List<Dictionary<string, string>> chatHistory)
        {

            var messages = new List<object>
            {
                new {
                    role = "system",
                    content = "You are a professional mental health assistant. Answer directly and specifically to every question. Do not include greetings or introductions. Always provide clear, concise, and actionable answers. Keep responses focused only on the user's question."
                }
            };


            messages.AddRange(chatHistory.Select(msg => new { role = msg["role"], content = msg["content"] }));

            var payload = new
            {
                model = "openai/gpt-4.1-nano",
                messages,
                stream = false
            };

            var request = new HttpRequestMessage(HttpMethod.Post, apiUrl)
            {
                Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Authorization", $"Bearer {apiKey}");

            try
            {
                var response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
                    return jsonResponse.choices[0].message.content.ToString().Trim();
                }
                return $"Error: {response.StatusCode}";
            }
            catch (Exception ex)
            {
                return $"Exception: {ex.Message}";
            }
        }



    }
}

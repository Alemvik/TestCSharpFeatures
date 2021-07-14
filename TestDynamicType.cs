using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TestDynamicType { // https://www.youtube.com/watch?v=VyGAEbmiWjE&t=729s
	static class Tester {
		public static async Task Go(string githubUser_a = "Alemvik")
		{
			Console.WriteLine($"\n--- TestDynamicType {new String('-', 50)}\n");

			var httpClient = new HttpClient {
				DefaultRequestHeaders = {
					Accept = {new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json")},
					UserAgent = {ProductInfoHeaderValue.Parse("request")}
				}
			};

			var responseText = await httpClient.GetStringAsync("https://api.github.com/users/" + githubUser_a);
			// dynamic response = System.Text.Json.JsonSerializer.Deserialize<object>(responseText);
			dynamic response = Newtonsoft.Json.JsonConvert.DeserializeObject(responseText);

			//Console.WriteLine(responseText);

			Console.WriteLine($"On GitHub, {githubUser_a} has {response.followers} followers");
		}
	}
}
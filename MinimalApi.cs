using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using Carter;
//using Carter.OpenApi;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

using Alemvik;

// https://github.com/CarterCommunity/Carter
// https://weblog.west-wind.com/posts/2016/sep/28/external-network-access-to-kestrel-and-iis-express-in-aspnet-core
// https://www.youtube.com/watch?v=wvpuyQANHog
namespace Test {
	public class MinimalApi {
		public MinimalApi() 
		{
			var msg = "Test Minimal API";
			Console.WriteLine($"\n--- {msg} {new String('-', Math.Max(65-msg.Length,3))}\n");

			var webHost = new WebHostBuilder()
				.UseKestrel()
				.UseUrls("http://*:5000", "https://*:5001")
				.ConfigureServices(services => {
					services.AddCarter();
				})
				.Configure(app => {
					app.UseRouting();
					app.UseEndpoints(UriBuilder => UriBuilder.MapCarter());
				})
				.Build();

			webHost.Run();
		}
	}

	public class ApiMethods : CarterModule
	{
		public ApiMethods()
		{
			Get("/hello", async (request, response) => {
				await response.WriteAsync(@"<html>
					<head>
						<title>MinApi</title>
						<style>
							h1 {
								color: red;
							}
						</style>
					</head>
					<body>
						<h1>Hello world !</h1>
					</body>
				</html>");
			});

			Get("/donut", async (request, response) => {
				await response.WriteAsync(System.IO.File.ReadAllText("Donut.html"));
			});
		}
	}
	/*public class GetDonut : Carter.OpenApi.RouteMetaData
	{
		public override string Description { get; } = "Draws a donut in a canvas section";

		public override RouteMetaDataResponse[] Responses { get; } =
		{
			new RouteMetaDataResponse {
				Code = 200,
				Description = $"Just a donut",
				Response = typeof(string)
			}
		};

		public override string Tag { get; } = "Donut";
	}*/
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace ConsoleApp
{
    class Program
    {
        private static readonly HttpClient clientNbu = new HttpClient();
        private static readonly HttpClient clientGA = new HttpClient();
        private static readonly Uri nbuUrl = new Uri("https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange?json");
        private static readonly Uri gaUrl = new Uri("http://www.google-analytics.com/collect");

        static async Task Main(string[] args)
        {
            Console.WriteLine("Start work!");
            clientNbu.DefaultRequestHeaders.Accept.Clear();
            clientNbu.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            clientGA.DefaultRequestHeaders.Accept.Clear();
            clientGA.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            clientGA.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("CurrencyExchange", "1.0"));

            var cancellationToken = new CancellationTokenSource();

            Console.CancelKeyPress += (sender, e) =>
            {
                Console.WriteLine("Canceling...");
                cancellationToken.Cancel();
                e.Cancel = true;
            };

            await Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    cancellationToken.Token.ThrowIfCancellationRequested();

                    var response = await clientNbu.GetAsync(nbuUrl);

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Response are ok!");
                        var currencyExchanges = await response.Content.ReadAsAsync<List<CurrencyExchangeResponse>>();
                        var usd = currencyExchanges.Find(q => q.r030 == 840);

                        if (usd != default)
                        {
                            Console.WriteLine($"Current {usd.cc} exchange {usd.rate} UAH per 1 {usd.cc}");
                            await Track(usd.rate);
                        }
                        else
                        {
                            Console.WriteLine("Not found USD exchange");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Bad response: {response.StatusCode} - {response.ReasonPhrase}");
                    }

                    await Task.Delay((int)TimeSpan.FromSeconds(30).TotalMilliseconds, cancellationToken.Token);
                }
            }, cancellationToken.Token);
        }

        private static async Task Track(float value)
        {
            if (value == default) throw new ArgumentNullException("value");

            Console.WriteLine($"Sending to GA!");

            var data = new GAMPObject(value.ToString())
                .AsDictionary()
                .Aggregate("", (data, next) => string.Format("{0}&{1}={2}", data, next.Key, HttpUtility.UrlEncode(next.Value)))
                .TrimStart('&')
                .TrimEnd('&');

            var response = await clientGA.PostAsync(gaUrl, new StringContent(data));

            if (response.IsSuccessStatusCode) Console.WriteLine("GA data sended!");
            else Console.WriteLine($"Bad response from GA: {response.StatusCode} - {response.ReasonPhrase}");
        }
    }
}

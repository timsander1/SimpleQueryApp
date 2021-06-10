using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace ExampleQueryApp
{
    class ExampleApp
    {
        public CosmosClient client;
        public Container container;
        public string connectionString;
        public string databaseId;
        public string containerId;
        public string partitionKeyPath;
        public string partitionKeyValue;
        public ItemRequestOptions itemRequestOptions;
        public QueryRequestOptions queryRequestOptions;
        public ConnectionMode connectionMode;

        public async Task Run()
        {
      
            this.connectionString = "";
            this.databaseId = "ShoppingDatabase";
            this.containerId = "ShoppingContainer";
            this.partitionKeyPath = "/myPartitionKey";

            this.connectionMode = ConnectionMode.Gateway;

            try
            {
                this.client = new CosmosClient(this.connectionString, new CosmosClientOptions { ConnectionMode = this.connectionMode });
                this.queryRequestOptions = new QueryRequestOptions { ConsistencyLevel = ConsistencyLevel.Eventual };
                this.container = this.client.GetContainer(this.databaseId, this.containerId);
                await this.container.ReadContainerAsync();  //ReadContainer to see if it is created
            }
            catch
            {
                // If container has not been created, create it
                Database database = await this.client.CreateDatabaseIfNotExistsAsync(this.databaseId);
                Container container = await database.CreateContainerIfNotExistsAsync(this.containerId, this.partitionKeyPath, 6000);
                this.container = container;
            }
            await RunQueries(this);
        }

        public static async Task RunQueries(ExampleApp exampleApp)
        {

            int currentTest = 0;
            int totalTests = 100;
            string sqlQuery = "SELECT TOP 10 c.id FROM c WHERE CONTAINS(c.Item, \"Socks" + "\")";

            List<Result> results = new List<Result>(); //Individual Benchmark results
            Stopwatch stopwatch = new Stopwatch();


            // Simple query
            QueryDefinition query = new QueryDefinition(sqlQuery);

            await Console.Out.WriteLineAsync("Press any key to stop");

            while (!Console.KeyAvailable)
            {
                FeedIterator<Item> resultSetIterator = exampleApp.container.GetItemQueryIterator<Item>(
                query, requestOptions: exampleApp.queryRequestOptions);

                double requestCharge = 0;

                stopwatch.Start();

                while (resultSetIterator.HasMoreResults)
                {
                    FeedResponse<Item> response = await resultSetIterator.ReadNextAsync();
                    requestCharge = requestCharge + response.RequestCharge;
                }

                stopwatch.Stop();
                currentTest++;

                Console.WriteLine($"Query execution: {currentTest}, Query: {sqlQuery}, Latency: {stopwatch.ElapsedMilliseconds} ms, Request Charge: {requestCharge} RUs");

                results.Add(new Result(stopwatch.ElapsedMilliseconds, requestCharge));

                stopwatch.Reset();
            }

            OutputResults(exampleApp, results);
        }

        private static void OutputResults(ExampleApp exampleApp, List<Result> results)
        {
            string averageLatency = Math.Round(results.OrderBy(o => o.Latency).Take(99).Average(o => o.Latency), 1).ToString();
            string averageRu = Math.Round(results.OrderBy(o => o.Latency).Take(99).Average(o => o.RU), 1).ToString();

            Console.WriteLine($"\nSummary\n");
            Console.WriteLine($"Average Latency:\t{averageLatency} ms");
            Console.WriteLine($"Average Request Units:\t{averageRu} RUs\n\nPress any key to continue...\n");
            Console.ReadKey(true);
        }
    }
    class Result
    {
        public long Latency;
        public double RU;

        public Result(long latency, double ru)
        {
            Latency = latency;
            RU = ru;
        }
    }
}

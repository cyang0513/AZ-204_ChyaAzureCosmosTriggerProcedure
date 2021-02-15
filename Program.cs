using System;
using System.Configuration.Internal;
using System.IO;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Scripts;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace ChyaAzureCosmosTriggerProcedure
{
   class Program
   {
      static IConfiguration m_Config;
      static void Main(string[] args)
      {
         m_Config = new ConfigurationBuilder().AddJsonFile(@".\appsettings.json", true, true).Build();
         Console.WriteLine("CHYA Comsmos Procedure and trigger...");

         var cosmosConn = m_Config.GetSection("CosmosConn").Value;
         var cosmosAuth = m_Config.GetSection("CosmosAuth").Value;
         var cosmosDB = m_Config.GetSection("CosmosDB").Value;
         var cosmosContainer = m_Config.GetSection("CosmosContainer").Value;

         var cosmos = new CosmosClient(cosmosConn, cosmosAuth);

         var db = cosmos.GetDatabase(cosmosDB);
         Console.WriteLine("Cosmos DB:" + cosmosDB);

         var container = db.GetContainer(cosmosContainer);
         Console.WriteLine("Container:" + container.Id);

         var taskP = TestProcedure(container);
         Task.WaitAny(taskP);

         var taskE = ExecProcedure(container);
         Task.WaitAny(taskE);

         var taskT = TestTrigger(container);
         Task.WaitAny(taskT);

         Console.ReadKey();

      }

      static async Task TestTrigger(Container container)
      {
         Console.WriteLine("Creating trigger...");

         try
         {
            var triggerCreate = await container.Scripts.CreateTriggerAsync(new TriggerProperties()
                                                                           {
                                                                              Id = "addPostTrigger",
                                                                              TriggerOperation = TriggerOperation.Create,
                                                                              TriggerType = TriggerType.Post,
                                                                              Body = File.ReadAllText(@".\Trigger\addCount.js"),
                                                                           });

            Console.WriteLine("Trigger created: " + triggerCreate.StatusCode);

         }
         catch (CosmosException e)
         {
            Console.WriteLine(e.StatusCode);
         }

         try
         {
            var triggerCreate = await container.Scripts.CreateTriggerAsync(new TriggerProperties()
                                                                           {
                                                                              Id = "dateTime",
                                                                              TriggerOperation = TriggerOperation.Create,
                                                                              TriggerType = TriggerType.Pre,
                                                                              Body = File.ReadAllText(@".\Trigger\dateTime.js"),
                                                                           });

            Console.WriteLine("Trigger created: " + triggerCreate.StatusCode);

         }
         catch (CosmosException e)
         {
            Console.WriteLine(e.StatusCode);
         }

      }

      static async Task TestProcedure(Container container)
      {
         Console.WriteLine("Binding procedure...");

         try
         {
            var create = await container.Scripts.CreateStoredProcedureAsync(new StoredProcedureProperties()
                                                                            {
                                                                               Id = "createMetaData",
                                                                               Body = File.ReadAllText(
                                                                                  @".\Procedure\createMetaData.js")
                                                                            });
            Console.WriteLine("Procedure creation: " + create.StatusCode);
         }
         catch (CosmosException e)
         {
            Console.WriteLine(e.StatusCode);
         }

      }

      static async Task ExecProcedure(Container container)
      {
         Console.WriteLine("Exec procedure...");

         var item1 = JsonConvert.SerializeObject(new MetaData()
                                                {
                                                   id = "_metadata",
                                                   City = "Stockholm",
                                                   AddCount = 0,
                                                   ModifyCount = 0,
                                                   DelCount = 0
                                                });
         var item2 = JsonConvert.SerializeObject(new MetaData()
                                                 {
                                                    id = "_metadata2",
                                                    City = "Lund",
                                                    AddCount = 0,
                                                    ModifyCount = 0,
                                                    DelCount = 0
                                                 });

         try
         {
            var exec = await container.Scripts.ExecuteStoredProcedureAsync<string>(
               "createMetaData", new PartitionKey("Stockholm"), new[]
                                                                {
                                                                   new dynamic[] {item1}
                                                                });
            Console.WriteLine("Procedure executed: " + exec.StatusCode);

         }
         catch (CosmosException e)
         {
            Console.WriteLine(e.StatusCode + " " + e.SubStatusCode);
         }

         try
         {
            var exec2 = await container.Scripts.ExecuteStoredProcedureAsync<string>(
               "createMetaData", new PartitionKey("Lund"), new[]
                                                           {
                                                              new dynamic[] {item2}
                                                           });

            Console.WriteLine("Procedure executed: " + exec2.StatusCode);
         }
         catch (CosmosException e)
         {
            Console.WriteLine(e.StatusCode + " " + e.SubStatusCode);
         }

      }
   }

   [Serializable]
   class MetaData
   {
      [JsonProperty("id")]
      public string id { get; set; }

      public string City { get; set; }

      public int AddCount { get; set; }
      public int ModifyCount { get; set; }
      public int DelCount { get; set; }

   }
}

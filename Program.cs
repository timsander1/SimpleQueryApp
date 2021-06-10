using System;
using System.Threading.Tasks;

namespace ExampleQueryApp
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            ExampleApp exampleApp = new ExampleApp();
            await exampleApp.Run();
        }
    }
}

using System;
using redis = StackExchange.Redis;

namespace ConsoleApp1
{
    static class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Cheguei...");

                // PublishSubscriver();
                // AdicionaEmCache();

                Trabalho();

                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void Trabalho()
        {
            try
            {
                // var connection = redis.ConnectionMultiplexer.Connect("localhost");

                var connection = GetConection("13.65.194.91:8081");

                var subscriber = connection.GetSubscriber();
                var database = connection.GetDatabase();

                // pub.Subscribe("Perguntas", (canal, mensagem) =>
                // {
                //  Console.WriteLine(mensagem.ToString());
                // });

                var perguntas = subscriber.Subscribe("Perguntas");

                // perguntas.OnMessage(x => {
                // Console.WriteLine(x.Message.ToString());
                // });


                subscriber.Subscribe("Perguntas", (ch, msg) =>
                {
                    var numeroPergunta = msg.ToString().Substring(0, msg.ToString().IndexOf(":"));
                    database.HashSet(numeroPergunta, "Salvador", "Barril Dobrado");
                    Console.WriteLine(msg.ToString());
                });
            }
            catch 
            {
                throw;
            }
        }

        private static void PublishSubscriver()
        {
            try
            {
                var connection = GetConection();

                var subscriber = connection.GetSubscriber();

                Console.WriteLine("Informar a mensagem");
                subscriber.Publish("FIAP16", Console.ReadLine());

                Console.WriteLine("Deseja enviar uma nova mensagem? S / N");
                var retorno = Console.ReadLine();

                if ("S".Equals(retorno, StringComparison.OrdinalIgnoreCase))
                    PublishSubscriver();
            }
            catch
            {
                throw;
            }
        }

        private static void AdicionaEmCache()
        {
            try
            {
                var connection = GetConection();
                var database = connection.GetDatabase();

                var _adicionando = database.StringSet("AndersonII", "Uma lista de nomes");

                var _recebendo = database.StringGet("AndersonII");

                Console.WriteLine(_recebendo);
            }
            catch
            {
                throw;
            }
        }

        private static redis.ConnectionMultiplexer GetConection(string conexao = "localhost")
        {
            return redis.ConnectionMultiplexer.Connect(conexao);
        }
    }
}

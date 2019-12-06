using System;
using redis = StackExchange.Redis;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace ConsoleApp1
{
    static class Program
    {
        class Pergunta
        {
            public string id;
            public string pergunta;
            public string resposta;
        }

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Ouvindo perguntas...");

                PublicaMensagemRabbit();
                PublicaMensagemRedis();

                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void PublicaMensagemRabbit()
        {
            try
            {
                //Conexão com Redis de perguntas
                var connection = GetConection("13.65.194.91:6379");

                var subscriber = connection.GetSubscriber();
                var database = connection.GetDatabase();

                var perguntas = subscriber.Subscribe("Perguntas");

                //Captura pergunta e envia para um exchange no Rabbit
                subscriber.Subscribe("Perguntas", (ch, msg) =>
                {
                    InsereMensagemRabbit(msg);
                });
            }
            catch
            {
                throw;
            }
        }

        private static void PublicaMensagemRedis()
        {
            //Conexao com o Rabbit
            var factory = new ConnectionFactory()
            {
                UserName = "teste",
                Password = "teste",
                HostName = "10.20.34.31"
            };

            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var msg = Encoding.UTF8.GetString(body);
                InsereMensagemRedis(msg);
            };

        }

        private static void InsereMensagemRedis(string msg)
        {
            var connection = GetConection("13.65.194.91:6379");

            var subscriber = connection.GetSubscriber();
            var database = connection.GetDatabase();
            var pergunta = JsonConvert.DeserializeObject<Pergunta>(msg);
            database.HashSet(pergunta.id, "output_pergunta", pergunta.resposta);
        }

        private static void InsereMensagemRabbit(string msg)
        {
            //Conexao com Rabbit
            var factory = new ConnectionFactory()
            {
                UserName = "teste",
                Password = "teste",
                HostName = "10.20.34.31"
            };

            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            //Trata a msg
            var json = criaJsonPergunta(msg);
            var body = System.Text.Encoding.UTF8.GetBytes(json);

            //Publica a msg
            channel.BasicPublish(exchange: "input_pergunta",
            routingKey: "",
            basicProperties: null,
            body: body);
        }

        private static string criaJsonPergunta(string msg)
        {
            var pergunta = new Pergunta();
            var split = msg.Split(":");
            pergunta.id = split[0];
            pergunta.pergunta = split[1];
            pergunta.resposta = "";

            return Newtonsoft.Json.JsonConvert.SerializeObject(pergunta);
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

        private static redis.ConnectionMultiplexer GetConection(string conexao = "localhost")
        {
            return redis.ConnectionMultiplexer.Connect(conexao);
        }
    }
}

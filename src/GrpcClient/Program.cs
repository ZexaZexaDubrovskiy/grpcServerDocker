using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using GrpcServer;
using Google.Protobuf.WellKnownTypes;
using System.Net.Http;

class Program
{
    static async Task Main(string[] args)
    {
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var serverAddress = configuration["ServerAddress"];

        if (string.IsNullOrEmpty(serverAddress))
        {
            Console.WriteLine("Ошибка: не указан адрес сервера в конфигурации.");
            return;
        }

        var httpHandler = new HttpClientHandler();

        var channel = GrpcChannel.ForAddress(serverAddress, new GrpcChannelOptions
        {
            HttpHandler = httpHandler
        });

        var client = new DataService.DataServiceClient(channel);

        if (!int.TryParse(configuration["TotalPackets"], out int totalPackets))
        {
            Console.WriteLine("Ошибка: некорректное или отсутствующее значение 'TotalPackets' в конфигурации.");
            return;
        }

        if (!int.TryParse(configuration["RecordsInPacket"], out int recordsInPacket))
        {
            Console.WriteLine("Ошибка: некорректное или отсутствующее значение 'RecordsInPacket' в конфигурации.");
            return;
        }

        if (!int.TryParse(configuration["TimeInterval"], out int timeInterval))
        {
            Console.WriteLine("Ошибка: некорректное или отсутствующее значение 'TimeInterval' в конфигурации.");
            return;
        }

        for (int packetSeqNum = 1; packetSeqNum <= totalPackets; packetSeqNum++)
        {
            var packet = new Packet
            {
                PacketTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                PacketSeqNum = packetSeqNum,
                NRecords = recordsInPacket,
            };

            for (int i = 0; i < recordsInPacket; i++)
            {
                packet.PacketData.Add(new Data
                {
                    Decimal1 = 1.0 + i,
                    Decimal2 = 2.0 + i,
                    Decimal3 = 3.0 + i,
                    Decimal4 = 4.0 + i,
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                });
            }

            try
            {
                var reply = await client.SendPacketAsync(packet);
                Console.WriteLine($"Пакет {packetSeqNum} успешно отправлен.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отправке пакета {packetSeqNum}: {ex.Message}");
            }

            await Task.Delay(TimeSpan.FromSeconds(timeInterval));
        }
    }
}

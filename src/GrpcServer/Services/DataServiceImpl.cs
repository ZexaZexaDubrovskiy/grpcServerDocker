using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using GrpcServer;

namespace GrpcServer.Implementation
{
    public class DataServiceImpl : DataService.DataServiceBase
    {
        private readonly ILogger<DataServiceImpl> _logger;
        private readonly IConfiguration _configuration;

        public DataServiceImpl(ILogger<DataServiceImpl> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public override async Task<Empty> SendPacket(Packet request, ServerCallContext context)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();

                var command = new NpgsqlCommand(
                    @"INSERT INTO grpc_data 
                    (PacketSeqNum, RecordSeqNum, PacketTimestamp, Decimal1, Decimal2, Decimal3, Decimal4, RecordTimestamp) 
                    VALUES 
                    (@PacketSeqNum, @RecordSeqNum, to_timestamp(@PacketTimestamp / 1000.0), @Decimal1, @Decimal2, @Decimal3, @Decimal4, to_timestamp(@RecordTimestamp / 1000.0))",
                    connection);

                command.Parameters.Add("@PacketSeqNum", NpgsqlTypes.NpgsqlDbType.Integer);
                command.Parameters.Add("@RecordSeqNum", NpgsqlTypes.NpgsqlDbType.Integer);
                command.Parameters.Add("@PacketTimestamp", NpgsqlTypes.NpgsqlDbType.Bigint);
                command.Parameters.Add("@Decimal1", NpgsqlTypes.NpgsqlDbType.Numeric);
                command.Parameters.Add("@Decimal2", NpgsqlTypes.NpgsqlDbType.Numeric);
                command.Parameters.Add("@Decimal3", NpgsqlTypes.NpgsqlDbType.Numeric);
                command.Parameters.Add("@Decimal4", NpgsqlTypes.NpgsqlDbType.Numeric);
                command.Parameters.Add("@RecordTimestamp", NpgsqlTypes.NpgsqlDbType.Bigint);

                int recordSeqNum = 0;
                foreach (var data in request.PacketData)
                {
                    command.Parameters["@PacketSeqNum"].Value = request.PacketSeqNum;
                    command.Parameters["@RecordSeqNum"].Value = recordSeqNum++;
                    command.Parameters["@PacketTimestamp"].Value = request.PacketTimestamp; // в миллисекундах
                    command.Parameters["@Decimal1"].Value = data.Decimal1;
                    command.Parameters["@Decimal2"].Value = data.Decimal2;
                    command.Parameters["@Decimal3"].Value = data.Decimal3;
                    command.Parameters["@Decimal4"].Value = data.Decimal4;
                    command.Parameters["@RecordTimestamp"].Value = data.Timestamp; // в миллисекундах

                    await command.ExecuteNonQueryAsync();
                }

                return new Empty();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отправке пакета");
                throw new RpcException(new Status(StatusCode.Internal, "Ошибка на сервере при работе с базой данных"));
            }
        }
    }
}

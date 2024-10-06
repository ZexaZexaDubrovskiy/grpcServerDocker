# grpcServerDocker

1. Для запуска удалите старые образы(если есть)
<b>docker-compose down</b>

2. Соберите и запустите проект
<b>docker-compose up --build</b>

#Скачать образа
docker pull zexazexadubrovskiy/grpc-client
docker pull zexazexadubrovskiy/grpc-server


#Настройка после установки

<h2>docker/dockerfile</h2>

FROM ubuntu:24.04

RUN apt-get update && apt-get install -y \
    postgresql-12 \
    postgresql-contrib && \
    apt-get clean

COPY init.sh /docker-entrypoint-initdb.d/
COPY init.sql /docker-entrypoint-initdb.d/

RUN chmod +x /docker-entrypoint-initdb.d/init.sh

ENTRYPOINT ["/docker-entrypoint-initdb.d/init.sh"]

<h2>docker/init.sh</h2>

exec /usr/lib/postgresql/14/bin/postgres -D /var/lib/postgresql/14/main

<h2>docker/init.sql</h2>

CREATE TABLE grpc_data (
    PacketSeqNum INT,
    RecordSeqNum INT,
    PacketTimestamp TIMESTAMP,
    Decimal1 DECIMAL,
    Decimal2 DECIMAL,
    Decimal3 DECIMAL,
    Decimal4 DECIMAL,
    RecordTimestamp TIMESTAMP
);

<h2>docker-compose.yml</h2>

version: '3.8'
services:
  grpc_server:
    build:
      context: ./src/GrpcServer
      dockerfile: Dockerfile
    ports:
      - "5001:5001"
    depends_on:
      - grpc_postgres
    environment:
      ASPNETCORE_URLS: http://+:5001
      ConnectionStrings__DefaultConnection: "Host=grpc_postgres;Port=5432;Database=grpc_db;Username=postgres;Password=postgres"
    networks:
      - grpc_network

  grpc_client:
    build:
      context: ./src/GrpcClient
      dockerfile: Dockerfile
    depends_on:
      - grpc_server
    networks:
      - grpc_network

  grpc_postgres:
    image: postgres:14
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
      POSTGRES_DB: grpc_db
    volumes:
      - ./docker/init.sql:/docker-entrypoint-initdb.d/init.sql
    ports:
      - "5432:5432"
    networks:
      - grpc_network

networks:
  grpc_network:
    driver: bridge


<h1>Иерахрия после настройки</h1>
│   docker-compose.yml
│
├───docker
│       Dockerfile
│       init.sh
│       init.sql
│
└───src
    ├───GrpcClient
    │   │   appsettings.json
    │   │   Dockerfile
    │   │   GrpcClient.csproj
    │   │   Program.cs
    │   │
    │   └───Protos
    │           data.proto
    │
    └───GrpcServer
        │   appsettings.Development.json
        │   appsettings.json
        │   Dockerfile
        │   GrpcServer.csproj
        │   Program.cs
        │
        ├───Properties
        │       launchSettings.json
        │
        ├───Protos
        │       data.proto
        │       greet.proto
        │
        └───Services
                DataServiceImpl.cs

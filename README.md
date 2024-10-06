<h1>gRPC Server and Client Project</h1>

<p>Этот проект демонстрирует работу gRPC сервера и клиента, развернутых в контейнерах Docker, с использованием PostgreSQL для хранения данных. Проект состоит из gRPC сервера, который принимает пакеты данных от клиента и сохраняет их в базе данных, и gRPC клиента, который отправляет пакеты данных на сервер.</p>

<h2>Установка и запуск проекта</h2>

<h3>Шаг 1: Удалите старые образы (если есть)</h3>
<p>Если у вас уже были запущены образы или контейнеры, сначала выполните команду, чтобы остановить их и удалить:</p>

<pre><code>docker-compose down</code></pre>

<h3>Шаг 2: Соберите и запустите проект</h3>
<p>Для сборки и запуска проекта выполните команду:</p>

<pre><code>docker-compose up --build</code></pre>

<h3>Шаг 3: Скачивание Docker образов</h3>
<p>Вы также можете скачать образы сервера и клиента напрямую с Docker Hub:</p>

<pre><code>docker pull zexazexadubrovskiy/grpc-client<br>
docker pull zexazexadubrovskiy/grpc-server</code></pre>

<h2>Настройка</h2>

<h3>Dockerfile для PostgreSQL</h3>
<p>Этот файл загружает PostgreSQL и инициализирует базу данных:</p>

<pre><code>FROM ubuntu:24.04

RUN apt-get update && apt-get install -y \
    postgresql-12 \
    postgresql-contrib && \
    apt-get clean

COPY init.sh /docker-entrypoint-initdb.d/
COPY init.sql /docker-entrypoint-initdb.d/

RUN chmod +x /docker-entrypoint-initdb.d/init.sh

ENTRYPOINT ["/docker-entrypoint-initdb.d/init.sh"]</code></pre>

<h3>init.sh</h3>
<p>Этот скрипт запускает PostgreSQL в контейнере:</p>

<pre><code>exec /usr/lib/postgresql/14/bin/postgres -D /var/lib/postgresql/14/main</code></pre>

<h3>init.sql</h3>
<p>SQL-скрипт для создания таблицы в базе данных:</p>

<pre><code>CREATE TABLE grpc_data (
    PacketSeqNum INT,
    RecordSeqNum INT,
    PacketTimestamp TIMESTAMP,
    Decimal1 DECIMAL,
    Decimal2 DECIMAL,
    Decimal3 DECIMAL,
    Decimal4 DECIMAL,
    RecordTimestamp TIMESTAMP
);</code></pre>

<h2>Docker Compose файл</h2>
<p><code>docker-compose.yml</code> используется для оркестрации всех сервисов (gRPC сервер, клиент и PostgreSQL):</p>

<pre><code>version: '3.8'
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
    driver: bridge</code></pre>

<h2>Структура проекта</h2>

<pre><code>├── docker-compose.yml
├── docker
│   ├── Dockerfile
│   ├── init.sh
│   └── init.sql
└── src
    ├── GrpcClient
    │   ├── appsettings.json
    │   ├── Dockerfile
    │   ├── GrpcClient.csproj
    │   ├── Program.cs
    │   └── Protos
    │       └── data.proto
    └── GrpcServer
        ├── appsettings.Development.json
        ├── appsettings.json
        ├── Dockerfile
        ├── GrpcServer.csproj
        ├── Program.cs
        ├── Properties
        │   └── launchSettings.json
        ├── Protos
        │   ├── data.proto
        │   └── greet.proto
        └── Services
            └── DataServiceImpl.cs</code></pre>

<h2>Шаги для тестирования</h2>

<ol>
    <li><strong>Скачайте образы</strong> с Docker Hub:
        <pre><code>docker pull zexazexadubrovskiy/grpc-client<br>docker pull zexazexadubrovskiy/grpc-server</code></pre>
    </li>
    <li><strong>Запустите проект с помощью Docker Compose</strong>:
        <pre><code>docker-compose up --build</code></pre>
    </li>
</ol>

<p>После успешного запуска, клиент начнет отправлять пакеты данных на сервер, а сервер сохранит их в базе данных PostgreSQL.</p>
</body>
</html>

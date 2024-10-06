FROM ubuntu:24.04

RUN apt-get update && apt-get install -y \
    postgresql-12 \
    postgresql-contrib && \
    apt-get clean

COPY init.sh /docker-entrypoint-initdb.d/
COPY init.sql /docker-entrypoint-initdb.d/

RUN chmod +x /docker-entrypoint-initdb.d/init.sh

ENTRYPOINT ["/docker-entrypoint-initdb.d/init.sh"]

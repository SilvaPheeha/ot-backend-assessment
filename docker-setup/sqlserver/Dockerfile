FROM mcr.microsoft.com/mssql/server:2022-latest

USER root
WORKDIR /usr/src/app

COPY --chmod=755 entrypoint.sh /usr/src/app/entrypoint.sh
COPY init.sql /usr/src/app/init.sql

RUN sed -i 's/\r$//' /usr/src/app/entrypoint.sh /usr/src/app/init.sql \
    && chown -R mssql /usr/src/app

USER mssql
EXPOSE 1433

ENTRYPOINT ["/usr/src/app/entrypoint.sh"]

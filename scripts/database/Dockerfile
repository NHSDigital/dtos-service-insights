FROM mcr.microsoft.com/mssql-tools

COPY ./database .

RUN ls -lah

ENTRYPOINT ["./db-setup-entrypoint.sh"]

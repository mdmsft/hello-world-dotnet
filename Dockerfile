FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine as sdk
WORKDIR /usr/local/tmp
ADD . .
RUN dotnet publish -c Release -o /usr/local/tmp/bin

FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine
WORKDIR /home/dotnet
RUN addgroup dotnet
RUN adduser -h /home/dotnet -G dotnet -D dotnet
COPY --from=sdk --chown=dotnet:dotnet /usr/local/tmp/bin ./
USER dotnet
EXPOSE 5001
ENV NAME=World
ENV TLS_PATH=/etc/ssl/certs/tls.pfx
ENTRYPOINT [ "dotnet", "HelloWorld.dll" ]
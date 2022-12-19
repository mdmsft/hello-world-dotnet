FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine as sdk
WORKDIR /usr/local/app
ADD . .
RUN dotnet publish -c Release -o /usr/local/app/bin

FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine
WORKDIR /app
COPY --from=sdk /usr/local/app/bin ./
USER nobody
EXPOSE 5001
VOLUME /etc/ssl/app
ENV NAME=World
ENV PFX_PATH=
ENV PFX_PASSWORD=
ENV PEM_CRT_PATH=
ENV PEM_KEY_PATH=
ENTRYPOINT [ "dotnet", "HelloWorld.dll" ]
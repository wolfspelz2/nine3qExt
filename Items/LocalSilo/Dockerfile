FROM mcr.microsoft.com/dotnet/core/runtime:3.1

WORKDIR /app
COPY ./bin/Release/netcoreapp3.1 /app
EXPOSE 11111
EXPOSE 30000
ENTRYPOINT ["dotnet", "LocalSilo.dll"]

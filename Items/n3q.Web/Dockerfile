FROM mcr.microsoft.com/dotnet/core/aspnet:3.1

WORKDIR /app
COPY ./bin/Release/netcoreapp3.1 /app
COPY ./wwwroot /wwwroot
EXPOSE 25343
ENTRYPOINT ["dotnet", "n3q.Web.dll"]

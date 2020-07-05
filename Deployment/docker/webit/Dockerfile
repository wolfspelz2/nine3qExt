FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /app
COPY . ./
RUN dotnet publish Items/n3q.WebIt/n3q.WebIt.csproj -c Release -o out


FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build /app/out . 
EXPOSE 80
ENTRYPOINT ["dotnet", "n3q.WebIt.dll"]

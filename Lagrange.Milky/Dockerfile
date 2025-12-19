FROM alpine:3.23 AS base
USER root
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
RUN apk update && apk add build-base
ARG BUILD_CONFIGURATION=Release
ARG GIT_SHA=Unknown
ENV DOCKER=true
WORKDIR /src
COPY ["Lagrange.Milky/Lagrange.Milky.csproj", "Lagrange.Milky/"]
COPY ["Lagrange.Codec/Lagrange.Codec.csproj", "Lagrange.Codec/"]
COPY ["Lagrange.Core/Lagrange.Core.csproj", "Lagrange.Core/"]
COPY ["Lagrange.Proto.Generator/Lagrange.Proto.Generator.csproj", "Lagrange.Proto.Generator/"]
COPY ["Lagrange.Proto/Lagrange.Proto.csproj", "Lagrange.Proto/"]
COPY ["Lagrange.Milky.Implementation.Api.Generator/Lagrange.Milky.Implementation.Api.Generator.csproj", "Lagrange.Milky.Implementation.Api.Generator/"]
RUN dotnet restore "Lagrange.Milky/Lagrange.Milky.csproj"
COPY . .
WORKDIR "/src/Lagrange.Milky"
RUN dotnet build "./Lagrange.Milky.csproj" -p:SourceRevisionId=$GIT_SHA -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Lagrange.Milky.csproj" -p:SourceRevisionId=$GIT_SHA -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
ENV DOCKER=true
COPY --from=publish /app/publish .
WORKDIR /data
ENTRYPOINT ["/app/Lagrange.Milky"]

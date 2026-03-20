FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine-aot AS build

WORKDIR /app/src

COPY --link Lagrange.Codec/*.csproj Lagrange.Codec/
COPY --link Lagrange.Core/*.csproj Lagrange.Core/
COPY --link Lagrange.Milky/*.csproj Lagrange.Milky/
COPY --link Lagrange.Milky.Implementation.Api.Generator/*.csproj Lagrange.Milky.Implementation.Api.Generator/
COPY --link Lagrange.Proto/*.csproj Lagrange.Proto/
COPY --link Lagrange.Proto.Generator/*.csproj Lagrange.Proto.Generator/
RUN dotnet restore Lagrange.Milky

COPY --link . .
RUN dotnet publish Lagrange.Milky -c Release -p:DebugType=none -o /app/bin


FROM alpine

WORKDIR /root

COPY --from=build /app/bin/ /usr/local/bin/
CMD ["Lagrange.Milky"]

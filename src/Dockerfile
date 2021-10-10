FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

WORKDIR /src

# First copy just .csproj-s, so restore doesn't depend on anything else
COPY *.sln */*.csproj ./
# We need to fix project location afterwards, as the above puts all project files
# in the root directory. We assume that project filename is always the same as directory name
RUN for PRJ in *.csproj; do DIR=${PRJ%.*}; mkdir $DIR; mv $PRJ $DIR/; done && \
    dotnet restore

# Copy remaining files
COPY . .

# Build
ARG config=Release
RUN dotnet publish Couchbase.Fake \
    --no-restore \
    --configuration $config \
    --output /app

# Create output
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["/app/Couchbase.Fake"]

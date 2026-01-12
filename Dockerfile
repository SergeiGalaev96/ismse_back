# Базовый образ Windows Server Core с .NET Framework 4.8
FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8

# Создаем рабочую директорию
WORKDIR /inetpub/wwwroot

# Копируем файлы проекта
COPY ISMSE-REST-API/ .

# (по желанию) Указываем переменные среды
ENV ASPNETCORE_ENVIRONMENT=Production

# Порт 80 для веб-приложения
EXPOSE 80

# IIS уже запущен по умолчанию в этом образе
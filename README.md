# ООП - Лабораторная работа №3
## Галсанов Булат M4105

Учебный проект на основе Web API (ASP.NET), созданный с использованием .NET Core 8.0.

--> [Техническое задание](https://docs.google.com/document/d/1Pqu6B-3KE_ydKBFI-2PB5lW3IeLheFn00yZDCoFi3e4/)

## Установка

Клонируйте репозиторий:
```sh
git clone https://github.com/Readiee/WebApiStoreOopLab3.git
```

Перейдите в директорию проекта:
```sh
cd .\WebApiStoreOopLab3\WebApiStoreOopLab3\
```

Установите зависимости:
```sh
dotnet restore
```

Создайте миграцию:
```sh
dotnet ef migrations add InitialCreate
```

Примените миграции к базе данных:
```sh
dotnet ef database update
```

Запустите проект:
```sh
dotnet run
```

Адрес Swagger UI:
```sh
/swagger/index.html
```

# MetadataParser
Репозиторий с утилитой контроля наполненности обязательных полей.

## Описание
Утилита позволяет проверить наполненность полей, обозначенных в прикладной разработке как обязательные, после процесса миграции документов и справочников.

## Использование
В папке с MetadataParser.exe заменить библиотеки Sungero.Domain.dll и Sungero.Domain.Shared.dll на аналогичные из опубликованной разработки, например, из папки Worker
Запустить MetadataParser.exe с параметрами:
* -s, --source - путь к папке с прикладной разработкой. Обязательный.
* -t, --target - путь к файлу, в который будут сохранены данные.

В результате файл target будет заполнен списком сущностей со всеми обязательными реквизитами:
```
# Commons

## City

City: City | en - City | ru - Населенный пункт

### Свойства:
*Наименование (Name) | Строка (250)
*Регион (Region) | Ссылка (Sungero.Commons.Region)
*Страна (Country) | Ссылка (Sungero.Commons.Country)
...
```
В папке файла target так же будет создан sql скрипт "ReportRequest.sql". При выполнении скрипта будет создана и наполнена таблица "RequiredFields".
![Alt text](/ScriptResult.png?raw=true "Результат выполнения ReportRequest.sql")

### Установка для использования на проекте

Возможные варианты:

A. Fork репозитория.
1. Сделать fork репозитория <Название репозитория> для своей учетной записи.
2. Склонировать созданный в п. 1 репозиторий в папку.

B. Копирование репозитория в систему контроля версий.
Рекомендуемый вариант.
1. В системе контроля версий с поддержкой git создать новый репозиторий.
2. Склонировать репозиторий MetadataParser в папку с ключом --mirror.
3. Перейти в папку из п. 2.
4. Импортировать клонированный репозиторий в систему контроля версий командой:
git push –mirror <Адрес репозитория из п. 1>

# SpaceProfiler
Сделано:
- параллельная загрузка дерева 
- корректная реакция на файлы без доступа
- простой UI с отображением фактических размеров, группировкой файлов, подсвечиванием крупных сущностей
- отслеживание изменений на диске:
  - добавление
  - изменение файлов
  - удаление
  - перемещение
  - смена прав доступа
- тесты на загрузку дерева, отслеживание изменений, добавление файлов

В первую очередь:
1. Пересчитывать % при изменении размера корня
2. Реагировать на удаление корня
3. Глобальная обработка ошибок
4. Отображать лоадер
5. Отображать текущий каталог
6. Добавить иконки (для приложения, для элементов)
7. Сортировка узлов по размеру
8. Сделать бенчмарк на загрузку большой папки с диска
9. Принять решение, нужно ли параллелить загрузку дерева с диска и сколько потоков лучше брать
10. Прокачать Readme

Во вторую очередь
1. Красивая табличка = починить выравнивание столбцов, добавить названия столбцов (?)
2. Не закрывать узел, если что-то поменялось, а править список детей
3. Контекстное меню с переходом в Explorer
4. Удаление через контекстное меню
5. Прогресс загрузки элемента 
6. Визуализация процента от родителя

Проверить:
- отображение файла в корне C: на 200 символов в названии
- загрузка диска С:
- изменение прав доступа
- закрытие программы до окончания загрузки с диска

Оставить на будущее:
1. Логирование
2. Размер файла на диске
3. Оптимизировать rename, чтобы не Delete+Create
4. Удаление из дерева без локов

Куча мыслей:
- можно добавить исключение определенных папок. Например, папка Windows большая, часто меняется (tmp файлы), а чистить ты её вряд ли будешь. Но нужно поизучать. Возможно стоит не прям исключать, а Watcher для них не включать.
- можно сделать ретраи на обработку узлов на случай исключений
- GetDirectoryName сейчас работает не оптимально, много строчек создаёт
- можно сделать группировку папок, чтобы ускорить для них сортировку, но надо подумать, как лучше
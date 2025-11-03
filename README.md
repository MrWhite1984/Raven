<h1>Raven — gRPC микросервис управления пользовательским контентом</h1>
<b>Raven</b> — это высоконагруженный микросервис для работы с постами, комментариями, категориями и тегами. Архитектура построена вокруг gRPC, PostgreSQL, Neo4j, MinIO, Redis и ML-компонента на Python/FastAPI. Проект полностью контейнеризован через Docker Compose.<br>
<hr>
<h2>Основные возможности</h2>
<ul>
<li>Полный CRUD для постов, комментариев, тегов и категорий</li>
<li>Интерактивные действия: лайки, просмотры, закладки</li>
<li>Персонализированные рекомендации на основе графа (Neo4j)</li>
<li>Автоподбор тегов к новым постам с помощью обучаемой ML-модели</li>
<li>Хранение медиафайлов в объектном хранилище MinIO</li>
<li>Поддержка древовидных комментариев (вложенные ответы)</li>
<li>Кэширование и логирование через Redis</li>
<li>Web-интерфейс для просмотра логов (Razor Pages)</li>
</ul>
<h2>Используемые технологии</h2>
<table>
  <tr>
    <th>Компонент</th>
    <th>Технология</th>
  </tr>
  <tr>
    <td>Основной сервис</td>
    <td>.NET Core, ASP.NET Core, gRPC</td>
  </tr>
  <tr>
    <td>Реляционные БД</td>
    <td>PostgreSQ</td>
  </tr>
  <tr>
    <td>Графовая БД</td>
    <td>Neo4j</td>
  </tr>
  <tr>
    <td>Объектное хранилище</td>
    <td>MinIO</td>
  </tr>
  <tr>
    <td>Кэш / Логи</td>
    <td>Redis</td>
  </tr>
  <tr>
    <td>ML-предсказатель тегов</td>
    <td>Python, Scikit-learn, FastAPI, TfidfVectorizer</td>
  </tr>
  <tr>
    <td>Контейнеризация</td>
    <td>Docker, Docker Compose</td>
  </tr>
  <tr>
    <td>Тестирование</td>
    <td>Postman, ghz (нагрузочное)</td>
  </tr>
</table>
<h2>Архитектура Raven</h2>
<image src="git_img/Architecture.png">
<h2>Компоненты Raven</h2>
<image src="git_img/Components.png">
<h2>Структура основной PostgreSQL базы данных</h2>
<image src="git_img/DB.png">


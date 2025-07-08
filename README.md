# System Parkingowy – Mikroserwisy

---

## 1. Stos technologiczny
- **Język:** C#
- **Framework:** .NET 8 (ASP.NET Core Web API)
- **Baza danych:** SQL Server LocalDB (każdy serwis ma własną bazę)
- **ORM:** Entity Framework Core
- **Testy:** xUnit (jednostkowe i integracyjne)
- **Diagramy:** Mermaid (ERD, REST, Use Case)
- **Narzędzia:** Swagger, Postman, Git

---

## 2. Instrukcja uruchamiania
1. **Wymagania:**
   - .NET 8 SDK
   - SQL Server LocalDB (domyślnie w Windows)
2. **Instalacja zależności:**
   ```powershell
   dotnet restore
   ```
3. **Tworzenie baz danych:**
   ```powershell
   dotnet ef database update --project UserService/UserService.csproj
   dotnet ef database update --project PromotionService/PromotionService.csproj
   dotnet ef database update --project ReservationService/ReservationService.csproj
   ```
4. **Uruchamianie serwisów (osobne konsole):**
   ```powershell
   dotnet run --project UserService/UserService.csproj --urls=https://localhost:5005
   dotnet run --project PromotionService/PromotionService.csproj --urls=https://localhost:5007
   dotnet run --project ReservationService/ReservationService.csproj --urls=https://localhost:5009
   ```
5. **Swagger:**
   - UserService: https://localhost:5005/swagger
   - PromotionService: https://localhost:5007/swagger
   - ReservationService: https://localhost:5009/swagger
6. **Testy:**
   ```powershell
   dotnet test
   ```
   ![image](https://github.com/user-attachments/assets/86651167-8821-4221-80d2-4e864a71b2bd)


---

## 3. **Każdy projekt (UserService, PromotionService, ReservationService) posiada własne testy jednostkowe i integracyjne.** 

## API Endpoints

### UserService

| Metoda | Endpoint                | Opis                                      |
|--------|-------------------------|-------------------------------------------|
| GET    | `/api/users`            | Pobierz listę wszystkich użytkowników     |
| GET    | `/api/users/{id}`       | Pobierz użytkownika o podanym ID          |
| POST   | `/api/users`            | Dodaj nowego użytkownika                  |
| PUT    | `/api/users/{id}`       | Zaktualizuj dane użytkownika o podanym ID |
| DELETE | `/api/users/{id}`       | Usuń użytkownika o podanym ID             |

### PromotionService

| Metoda | Endpoint                   | Opis                                         |
|--------|----------------------------|----------------------------------------------|
| GET    | `/api/promotions`          | Pobierz listę wszystkich promocji            |
| GET    | `/api/promotions/{id}`     | Pobierz promocję o podanym ID                |
| POST   | `/api/promotions`          | Dodaj nową promocję                          |
| PUT    | `/api/promotions/{id}`     | Zaktualizuj promocję o podanym ID            |
| DELETE | `/api/promotions/{id}`     | Usuń promocję o podanym ID                   |

### ReservationService

| Metoda | Endpoint                        | Opis                                         |
|--------|---------------------------------|----------------------------------------------|
| GET    | `/api/reservations`             | Pobierz listę wszystkich rezerwacji          |
| GET    | `/api/reservations/{id}`        | Pobierz rezerwację o podanym ID              |
| POST   | `/api/reservations`             | Dodaj nową rezerwację                        |
| PUT    | `/api/reservations/{id}`        | Zaktualizuj rezerwację o podanym ID          |
| DELETE | `/api/reservations/{id}`        | Usuń rezerwację o podanym ID                 |

---

![image](https://github.com/user-attachments/assets/ae0f0c32-f7a1-4fb8-b0c1-e58be3047990)
![image](https://github.com/user-attachments/assets/f932f2cc-6c08-4504-96a6-2c160557c553)
![image](https://github.com/user-attachments/assets/19451ddf-e7ea-413d-99a0-2839c567dcd1)

---

## 4. Diagram bazy danych (ERD)
![image](https://github.com/user-attachments/assets/9911ff9a-1247-436b-81db-fc77224d3109)

---

## 5. Diagram REST API (endpointy i zależności)

![image](https://github.com/user-attachments/assets/1fccb0dd-0401-4aea-beed-f570f2dc5050)


## 6. Diagram przypadków użycia (UML)

![image](https://github.com/user-attachments/assets/1103edea-4f37-4359-b353-184cf652f4c5)

---

## 7. Kluczowe elementy back-endu

### UserService
- **Kontroler:** UsersController (CRUD użytkowników)
- **Repozytorium:** UserRepository (operacje na bazie)
- **Model:** User, UserDTO

### PromotionService
- **Kontroler:** PromotionsController (CRUD promocji)
- **Repozytorium:** PromotionRepository
- **Model:** Promotion, PromotionDTO

### ReservationService
- **Kontroler:** ReservationsController (CRUD rezerwacji, walidacja user/promocja przez REST)
- **Repozytorium:** ReservationRepository
- **Model:** Reservation, PromotionDTO

---

## 8. Przypadki testowe (Gherkin)

### Przykład: Dodanie rezerwacji
```gherkin
Scenario: Dodanie nowej rezerwacji
  Given użytkownik istnieje w systemie
  And promocja istnieje w systemie
  When użytkownik wysyła żądanie POST do /api/reservations z poprawnymi danymi
  Then rezerwacja zostaje utworzona
  And odpowiedź zawiera status 200 OK
```

### Przykład: Próba rezerwacji z nieistniejącym użytkownikiem
```gherkin
Scenario: Dodanie rezerwacji dla nieistniejącego użytkownika
  Given użytkownik nie istnieje w systemie
  When użytkownik wysyła żądanie POST do /api/reservations
  Then odpowiedź zawiera status 400 Bad Request
  And rezerwacja nie zostaje utworzona
```

### Przykład: Usunięcie promocji
```gherkin
Scenario: Usunięcie istniejącej promocji
  Given promocja istnieje w systemie
  When użytkownik wysyła żądanie DELETE do /api/promotions/{id}
  Then promocja zostaje usunięta
  And odpowiedź zawiera status 200 OK
```

### Przykład: Aktualizacja użytkownika
```gherkin
Scenario: Aktualizacja danych użytkownika
  Given użytkownik istnieje w systemie
  When użytkownik wysyła żądanie PUT do /api/users/{id} z nowymi danymi
  Then dane użytkownika zostają zaktualizowane
  And odpowiedź zawiera status 200 OK
```

### Przykład: Pobranie wszystkich rezerwacji
```gherkin
Scenario: Pobranie listy rezerwacji
  Given istnieją rezerwacje w systemie
  When użytkownik wysyła żądanie GET do /api/reservations
  Then odpowiedź zawiera listę rezerwacji
  And odpowiedź zawiera status 200 OK
```

---

## 9. Wykaz źródeł i literatury
- Dokumentacja Microsoft: [ASP.NET Core](https://learn.microsoft.com/aspnet/core/)
- Dokumentacja EF Core: [Entity Framework Core](https://learn.microsoft.com/ef/core/)
- [Mermaid Live Editor](https://mermaid.live/)
- [Swagger](https://swagger.io/)
- [xUnit](https://xunit.net/)
- Własny kod i testy

---


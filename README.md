# Insurance Partners

Web aplikacija za upravljanje partnerima i njihovim policama osiguranja.

## Tehnologije

- **Backend:** ASP.NET Core 10 Minimal API, Dapper, EF Core (Identity), SignalR, Serilog, FluentValidation, JWT Bearer
- **Frontend:** JavaScript (ES moduli) + Vite, Bootstrap 4, SignalR JS klijent
- **Baza:** SQL Server LocalDB
- **Testovi:** xUnit, Moq, FluentAssertions

---

## Preduvjeti

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server LocalDB (dolazi s Visual Studio instalacijom)
- Node.js (za Vite — dev server i build frontenda)
- SSMS ili Azure Data Studio (za pokretanje SQL skripte)

---

## Postavljanje baze

1. Otvori SSMS i spoji se na `(localdb)\MSSQLLocalDB`
2. Otvori i pokreni cijelu skriptu:
   ```
   backend/SqlScripts/1_databaseCreate.sql
   ```
3. Skripta ce kreirati bazu `WienerPartners`, sve tablice i stored procedure

---

## Backend

### Konfiguracija

Otvori `backend/Partners.Api/appsettings.json` i postavi:

```json
{
  "ConnectionStrings": {
    "InsurancePartnersDb": "Server=(localdb)\\MSSQLLocalDB;Database=WienerPartners;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Secret": "POSTAVI_DUGACKI_RANDOM_KLJUC_MIN_32_ZNAKA",
    "Issuer": "InsurancePartners",
    "Audience": "InsurancePartnersClient"
  },
  "Identity": {
    "DefaultUser": {
      "Email": "manager@wiener.hr",
      "Password": "Manager123!"
    }
  },
  "Cors": {
    "AllowedOrigin": "http://localhost:3000"
  }
}
```

### Pokretanje

```bash
cd backend/Partners.Api
dotnet run
```

API je dostupan na `https://localhost:7146`.

Swagger dokumentacija: `https://localhost:7146/swagger`

### Defaultni korisnik

Pri prvom pokretanju aplikacija automatski kreira korisnika:

| Polje    | Vrijednost        |
|----------|-------------------|
| E-mail   | manager@wiener.hr |
| Lozinka  | Manager123!       |
| Uloga    | PolicyManager     |

---

## Frontend

Preduvjet: Node.js. Backend mora raditi na `:7146` (Vite proxy prosljeđuje `/api` i `/hubs`).

```bash
cd frontend
npm install      # samo prvi put — instalira Vite i ovisnosti
npm run dev      # dev server na http://localhost:3000
```

Otvori `http://localhost:3000/login.html` i prijavi se s defaultnim korisnikom.

Dev proxy preusmjerava `/api` i `/hubs` na `https://localhost:7146`, pa u developmentu **nema CORS-a ni prihvaćanja self-signed certifikata**.

### Produkcijski build

```bash
npm run build    # generira dist/ (statički bundle)
npm run preview  # lokalna provjera produkcijskog builda
```

---

## Testovi

Projekt ima dva test projekta:

### Partners.Core.Tests

Unit testovi za servise, validatore i pravila flagiranja. Ne zahtijevaju bazu.

```bash
cd tests/Partners.Core.Tests
dotnet test
```

### Partners.Api.Tests

Integracijski testovi za API endpointe. Koriste `WebApplicationFactory` s LocalDB bazom `WienerPartnersTest` koja se automatski kreira pri prvom pokretanju testova.

```bash
cd tests/Partners.Api.Tests
dotnet test
```

> **Napomena:** `WienerPartnersTest` baza kreira se automatski putem EF Core migracija. Nije potrebno rucno pokretati SQL skriptu za testnu bazu.

---

## Struktura projekta

```
InsurancePartners/
├── backend/
│   ├── Partners.Api/          # ASP.NET Core Web API
│   ├── Partners.Core/         # Domenska logika, servisi, validatori
│   ├── Partners.Dal/          # Repozitoriji (Dapper), EF Core (Identity)
│   └── SqlScripts/            # SQL skripta za kreiranje baze
├── frontend/
│   ├── package.json           # Vite + npm skripte (dev/build/preview)
│   ├── vite.config.js         # Vite config (dev proxy, build ulazi)
│   ├── css/                   # Stilovi
│   ├── js/                    # ES moduli (config, services, errors, helpers, partner, policy)
│   │   ├── main.js            # ulaz za index.html
│   │   └── login.js           # ulaz za login.html
│   ├── partials/              # HTML parcijali (forme, modali, liste)
│   ├── login.html             # Stranica za prijavu
│   └── index.html             # Glavna stranica (lista partnera)
└── tests/
    ├── Partners.Core.Tests/   # Unit testovi
    └── Partners.Api.Tests/    # Integracijski testovi
```

---

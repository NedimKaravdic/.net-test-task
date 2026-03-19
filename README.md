# Berg Exchange


## Tech Stack
- **Frontend**: Blazor Web App & Tailwind CSS.
- **Backend/Core**: .NET 10.0.
- **Baza**: MySQL.
- **Caching**: Redis.
- **Kontejnerizacija**: Docker.
- **(IaC)**: Terraform (Azure deployment spremno).

## Pokretanje sa Dockerom

1. Rename `.env.example` u `.env`, i unijeti API za `exchangerate.host`. i ostale podatke.
2. Docker Compose:
   ```bash
   docker compose up --build
   ```

## Lokalni run (Bez Dockera)
Za lokalno pokretanje MySQL i Redis serveri moraju biti aktivni, te

1. Update-ujte `appsettings.json` sa lokalnom konekcijom.
2. Pokrenite migracije baze:
   ```bash
   dotnet ef database update
   ```
3. Pokrenite aplikaciju:
   ```bash
   dotnet run
   ```


## Napomena
Terraform i Githgub workflow su dodani kao placeholderi za moguci Azure deployment i CI/CD.


## Autor
Projekt razvijen kao dio tehničkog zadatka za EHS d.o.o .

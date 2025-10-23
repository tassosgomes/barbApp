# Security Audit: Hardcoded Secrets

## 1. Executive Summary

A security audit of the BarbApp repository was conducted to identify hardcoded secrets. The audit revealed several instances of sensitive information, including database credentials, JWT secrets, and application passwords, stored in plaintext within the codebase. This practice poses a significant security risk, as it exposes the application and its data to unauthorized access.

This report details the findings of the audit and provides recommendations for remediating these vulnerabilities.

## 2. Findings

Hardcoded secrets were found in the following files:

| File | Location | Secret(s) |
|---|---|---|
| `backend/docker-compose.yml` | `/home/tsgomes/github-tassosgomes/barbApp/backend/docker-compose.yml` | - `ConnectionStrings__DefaultConnection`: Database password `4cd7e2e32c5994fcba50555430efe763`<br>- `JwtSettings__Secret`: JWT secret `c3df06950022492321151d5fdff463d351350166a50d3f94d4a86daa363d52e0`<br>- `POSTGRES_PASSWORD`: Database password `4cd7e2e32c5994fcba50555430efe763` |
| `backend/src/BarbApp.API/appsettings.json` | `/home/tsgomes/github-tassosgomes/barbApp/backend/src/BarbApp.API/appsettings.json` | - `ConnectionStrings.DefaultConnection`: Database credentials `Username=barbapp_user`, `Password=barbapp_password` |
| `README.md` | `/home/tsgomes/github-tassosgomes/barbApp/README.md` | - `BARBAPP_CONNECTION_STRING`: Database credentials `Username=postgres`, `Password=password` |
| `backend/scripts/test-e2e-flows.sh` | `/home/tsgomes/github-tassosgomes/barbApp/backend/scripts/test-e2e-flows.sh` | - Admin credentials: `admin@barbapp.com`/`Admin@123`<br>- Barbershop admin credentials: `admin@barbearia.com`/`Admin@123` |
| `backend/tests/BarbApp.IntegrationTests/DatabaseFixture.cs` | `/home/tsgomes/github-tassosgomes/barbApp/backend/tests/BarbApp.IntegrationTests/DatabaseFixture.cs` | - Test database credentials: `test_user`/`test_password` |
| `backend/tests/BarbApp.IntegrationTests/IntegrationTestWebAppFactory.cs` | `/home/tsgomes/github-tassosgomes/barbApp/backend/tests/BarbApp.IntegrationTests/IntegrationTestWebAppFactory.cs` | - Test JWT secret: `test-secret-key-at-least-32-characters-long-for-jwt`<br>- Test SMTP password: `test` |
| `docs/environment-variables.md` | `/home/tsgomes/github-tassosgomes/barbApp/docs/environment-variables.md` | - Example database credentials: `postgres`/`postgres`<br>- Example JWT secret: `FTVBUj5qEbI03He2NGDJjxZRsxvxmRwCr7EYZIINZSA=` |

## 3. Recommendations

It is strongly recommended to remove all hardcoded secrets from the codebase and utilize a secure method for managing them. The following actions should be taken:

1.  **Use Environment Variables:** For local development, store secrets in `.env` files. The existing `.gitignore` file already prevents these files from being checked into the repository. For production, use environment variables provided by the hosting platform (e.g., Docker, Azure App Service).

2.  **Utilize a Secret Management Service:** For production environments, it is best practice to use a dedicated secret management service such as [Azure Key Vault](https://azure.microsoft.com/en-us/products/key-vault), [AWS Secrets Manager](https://aws.amazon.com/secrets-manager/), or [HashiCorp Vault](https://www.vaultproject.io/). These services provide a secure way to store, manage, and rotate secrets.

3.  **Update Documentation:** The `docs/environment-variables.md` file should be updated to remove any example secrets and to provide clear instructions on how to set up the environment using environment variables or a secret management service.

4.  **Rotate Exposed Secrets:** Since the secrets have been exposed in the repository, they should be considered compromised. All exposed credentials should be rotated immediately. This includes database passwords, JWT secrets, and any other sensitive information.

## 4. Conclusion

The presence of hardcoded secrets in the BarbApp repository is a critical security vulnerability that requires immediate attention. By following the recommendations outlined in this report, the security of the application can be significantly improved.

# Configuring Secrets in Infisical SaaS

This guide provides a step-by-step process for configuring secrets in the Infisical SaaS platform for the BarbApp project. This will ensure that all sensitive information is stored securely and is accessible to the application in a safe manner.

## 1. Project and Environment Setup

1.  **Create a Project:**
    - Log in to your Infisical account.
    - Create a new project named `BarbApp`.

2.  **Create Environments:**
    - Inside the `BarbApp` project, create the following environments:
        - `dev` (for development)
        - `staging` (for testing)
        - `prod` (for production)

## 2. Adding Secrets

Navigate to the `dev` environment and add the following secrets. Repeat this process for the `staging` and `prod` environments, ensuring that the secret values are appropriate for each environment.

| Secret Name | Recommended Value (for `dev`) | Description |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | `Host=localhost;Database=barbapp_dev;Username=barbapp_dev;Password=<strong_dev_password>` | Database connection string. |
| `JwtSettings:Secret` | `<generate_a_32_char_random_string>` | JWT secret key. |
| `SmtpSettings:Host` | `smtp.mailtrap.io` | SMTP server host. |
| `SmtpSettings:Port` | `2525` | SMTP server port. |
| `SmtpSettings:Username` | `<mailtrap_username>` | SMTP username. |
| `SmtpSettings:Password` | `<mailtrap_password>` | SMTP password. |
| `R2Storage:AccessKeyId` | `<r2_access_key_id>` | Cloudflare R2 Access Key ID. |
| `R2Storage:SecretAccessKey` | `<r2_secret_access_key>` | Cloudflare R2 Secret Access Key. |
| `R2Storage:BucketName` | `barbapp-dev` | Cloudflare R2 bucket name. |

**Note:** For the `staging` and `prod` environments, use the actual credentials for your staging and production resources.

## 3. Creating a Machine Identity

To allow the BarbApp API to authenticate with Infisical and retrieve secrets, you need to create a machine identity.

1.  **Navigate to Machine Identities:**
    - In your `BarbApp` project, go to the "Machine Identities" section.

2.  **Create a New Machine Identity:**
    - Click on "Create Machine Identity".
    - Give it a descriptive name, such as `barbapp-api`.

3.  **Retrieve Client ID and Secret:**
    - After creating the machine identity, Infisical will provide you with a **Client ID** and a **Client Secret**.
    - **Important:** Copy the Client Secret immediately and store it in a safe place. You will not be able to see it again.

4.  **Set Environment Variables:**
    - These credentials will be used to set the `INFISICAL_CLIENT_ID` and `INFISICAL_CLIENT_SECRET` environment variables when running the application.

## 4. Mapping Secrets from `SECURITY_AUDIT.md`

The following table maps the secrets identified in the `SECURITY_AUDIT.md` report to the recommended secret names in Infisical.

| Original Location | Original Secret Name | Infisical Secret Name |
|---|---|---|
| `backend/docker-compose.yml` | `ConnectionStrings__DefaultConnection` | `ConnectionStrings:DefaultConnection` |
| `backend/docker-compose.yml` | `JwtSettings__Secret` | `JwtSettings:Secret` |
| `backend/docker-compose.yml` | `POSTGRES_PASSWORD` | (Part of `ConnectionStrings:DefaultConnection`) |
| `backend/src/BarbApp.API/appsettings.json` | `ConnectionStrings.DefaultConnection` | `ConnectionStrings:DefaultConnection` |
| `README.md` | `BARBAPP_CONNECTION_STRING` | `ConnectionStrings:DefaultConnection` |
| `backend/scripts/test-e2e-flows.sh` | `admin@barbapp.com`/`Admin@123` | (Should be seeded in the database, not a secret) |
| `backend/scripts/test-e2e-flows.sh` | `admin@barbearia.com`/`Admin@123` | (Should be seeded in the database, not a secret) |
| `backend/tests/BarbApp.IntegrationTests/DatabaseFixture.cs` | `test_user`/`test_password` | (Handled by test containers, no need to store in Infisical) |
| `backend/tests/BarbApp.IntegrationTests/IntegrationTestWebAppFactory.cs` | `test-secret-key-at-least-32-characters-long-for-jwt` | (Handled by in-memory configuration for tests) |
| `backend/tests/BarbApp.IntegrationTests/IntegrationTestWebAppFactory.cs` | `test` | (Handled by in-memory configuration for tests) |
| `docs/environment-variables.md` | `postgres`/`postgres` | `ConnectionStrings:DefaultConnection` |
| `docs/environment-variables.md` | `FTVBUj5qEbI03He2NGDJjxZRsxvxmRwCr7EYZIINZSA=` | `JwtSettings:Secret` |

## 5. Conclusion

By following this guide, you will have a secure and centralized place to manage all your application secrets. This will significantly improve the security posture of the BarbApp project.

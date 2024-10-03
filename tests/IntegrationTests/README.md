# Integration Tests Project

Welcome to the Integration Tests project. This project will contain automated tests that are designed to ensure that all components of the system work together as expected.

## Project Structure

```bash
IntegrationTests/
├── Config/
│   └── appsettings.json
├── Helpers/
│   ├── AppSettings.cs
│   ├── AzureFunctionRetryHelper.cs
│   ├── BlobStorageHelper.cs
│   ├── ConnectionStrings.cs
│   ├── DatabaseHelper.cs
│   ├── Endpoints.cs
│   ├── FilePaths.cs
│   └── JsonHelper.cs
├── Tests/
│   └── Integration/
│       └── EndToEndTests/
│           ├── E2E_FileUploadAndCreateEpisodeTest.cs
├── BaseIntegrationTest.cs
├── IntegrationTests.csproj
├── README.md
└── TestData/
    ├── test-happy-path.json
```

## Getting Started

### Prerequisites

- **.NET SDK 8.0** or higher
- **Visual Studio Code**
- **Git** installed on your machine
- **Azure Functions Core Tools**
- **Azure Data Studio**
- **Azurite**
- **Microsoft Azure Storage Explorer**
- **Podman**

### Setup Instructions

1. **Clone the Repository**

   ```bash
   git clone https://github.com/NHSDigital/dtos-service-insights.git
   ```

2. **Navigate to the IntegrationTests Directory**

   ```bash
   cd tests/IntegrationTests
   ```

3. **Restore NuGet Packages**

   ```bash
   dotnet restore
   ```

4. **Update Configuration**

   - Open `Config/appsettings.template.json` rename to `appsettings.json`and ensure the following settings are correct:

     - **Endpoints:** Verify that the API endpoints match your local environment.
     - **FilePaths:** Put your Json (or in future CSV file) in the TestData folder and confirm the paths to your test data files.
     - **BlobContainerName:** Set the name of your local Azure Blob Storage container.
     - **AzureWebJobsStorage:** Provide your Azure Storage connection string if local you can simply use ("UseDevelopmentStorage=true").
     - **ConnectionStrings:** Update `ServiceInsightsDbConnectionString` with your local connection string.

5. **Build the Project**

   ```bash
   dotnet build
   ```

6. **Launch Podman**
   Click the 'Set up' card and create a new machine, ensuring you allocate adequate resources (8Gb+ Memory Recommended). Ensure your machine is running, if not use 'Podman Machine Start' in terminal.

7. **Setup your local db**
   For detailed database setup, please reach out to the development team for instructions.

8. **Launch Azure Data Studio and create connection**
   Connect to Azure Data Studio and run the db scripts in scripts/database to create your local database and tables.

9. **Start Azurite**

   command shift + p to open command palette on Mac
   type 'azurite'
   ensure you have started Azurite Table Service, Azurite Queue Service and Azurite Blob Service

10. **Launch Microsoft Azure Storage Explorer**

11. **Create Local Settings**
    bash scripts/local-settings/create_local_settings.sh

12. **Start Azure Functions Locally**

    ```bash
    cd into each function folder and run 'func start'
    ```

13. **Run the Tests**

    ```bash
    dotnet test
    ```

## Project Components

### Configuration

- **appsettings.json**

  - Contains configuration settings such as endpoints, file paths, blob storage details, and database connection strings.

### Helpers

- **AppSettings.cs**

  - Configuration class which represents your `appsettings.json`.

- **AzureFunctionRetryHelper.cs**

  - Helper that implements retry logic for operations that may experience transient failures.

- **BlobStorageHelper.cs**

  - Provides methods for interacting with Azure Blob Storage (e.g. uploading files).

- **DatabaseHelper.cs**

  - Contains methods for database operations, such as cleaning the database before tests.

- **JsonHelper.cs**

  - Utilities for parsing JSON files and extracting data needed for tests.

- **ConnectionStrings.cs, Endpoints.cs, FilePaths.cs**

  - Additional configuration classes.

### Tests

- **E2E_FileUploadAndCreateEpisodeTest.cs**

  - An End-to-End test that uploads a valid JSON file to Blob Storage and verifies episodes are created in the database.

### BaseIntegrationTest.cs

- An abstract base class providing common setup for all integration tests, including dependency injection and configuration loading.

## Adding New Tests

1. **Create a New Test Class**

   - Place your new test class in the appropriate directory, e.g. `Tests/Integration/EndToEndTests/YourNewTest.cs`.

2. **Inherit from `BaseIntegrationTest`**

   ```csharp
   using Microsoft.VisualStudio.TestTools.UnitTesting;
   using IntegrationTests.Helpers;

   [TestClass]
   [TestCategory("Integration")]
   public class YourNewTest : BaseIntegrationTest
   {
       // Test methods go here
   }
   ```

3. **Implement Test Methods**

   - Use the `[TestMethod]` attribute for each test method.
   - Utilise helper classes for common operations.

4. **Run and Verify Your Test**

   ```bash
   dotnet test
   ```

## Best Practices

- **Do Not Log Sensitive Data**

  - Avoid logging any sensitive information such as IDs or personal data.

- **Use Helper Classes**

  - Leverage existing helpers to avoid code duplication.

- **Write Clear Assertions**

  - Provide meaningful messages in assertions to aid debugging.

- **Follow Coding Standards**

  - Maintain consistent naming conventions and code formatting.

# RestfulBooker Test Automation

## Overview

RestfulBooker is a C# test automation project designed for automated API and UI testing of the Restful Booker application. It utilizes SpecFlow for behavior-driven development, along with .NET 8 and industry-standard test automation practices. The repository is organized to support scalable, maintainable test suites and includes a CI/CD pipeline for automated test execution.

---

## Project Structure

- **Features/**: Contains `.feature` files describing test scenarios in Gherkin syntax.
- **Steps/**: Step definition files that implement scenario steps from feature files.
- **Hooks/**: Setup and teardown logic for tests, such as initialization and cleanup.
- **TestData/**: Test data files used for parameterized testing.
- **Utils/**: Utility classes and helper functions for shared logic.
- **RestfulBooker.csproj**: .NET project configuration file.
- **RestfulBooker.sln**: Solution file for Visual Studio.
- **specflow.json**: SpecFlow configuration file.
- **.github/workflows/dotnet-tests.yml**: GitHub Actions pipeline for automated build and test.

---

## Local Setup

1. **Prerequisites**
   - [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
   - [Git](https://git-scm.com/downloads)
   - (Optional) Visual Studio 2022+ or Visual Studio Code

2. **Clone the Repository**
   ```sh
   git clone https://github.com/ssujith91/RestfulBooker.git
   cd RestfulBooker
   ```

3. **Restore Dependencies**
   ```sh
   dotnet restore
   ```

4. **Build the Project**
   ```sh
   dotnet build --configuration Release
   ```

5. **Run Automated Tests**
   ```sh
   dotnet test --configuration Release --logger "trx;LogFileName=test_results.trx" --results-directory ./TestResults
   ```
   - Test results will be saved to the `TestResults` directory.

6. **(Optional) Generate HTML Test Report**
   ```sh
   dotnet tool install --global trx2html
   trx2html ./TestResults/test_results.trx ./TestResults/test_results.html
   ```

---

## CI/CD Pipeline (GitHub Actions)

The pipeline is defined in `.github/workflows/dotnet-tests.yml` and runs on every push or pull request to `main` or `master`. Key stages include:

1. **Code Checkout**
2. **Setup .NET 8**
3. **Restore Dependencies**
4. **Build Project**
5. **Run Tests & Generate TRX Report**
6. **Publish Test Results as Artifacts**
7. **(Optional) Convert TRX to HTML & Upload**

Artifacts (test results and HTML reports) are available for download in the Actions run summary.

---
## Manually Running the CI/CD Pipeline

You can manually trigger the GitHub Actions pipeline from the GitHub UI:

1. Navigate to the **Actions** tab in your repository.
2. Select the **.NET Test Automation CI** workflow from the left sidebar.
3. Click the **"Run workflow"** button.
4. Optionally, specify any input parameters (if defined).
5. Click **"Run workflow"** to start the pipeline manually.

This is useful for ad-hoc test runs, verifying changes before pushing, or rerunning tests after updating dependencies.

## Note on Test Failures

Some automated tests may fail because the Restful Booker API does not generate the expected responses for specific scenarios. These failures are primarily due to issues or faulty behavior in the API endpoints, not the test automation implementation. For transparency, comments have been added directly to the relevant scenarios in the feature files to indicate where SpecFlow tests fail due to unexpected API responses.

## Contributing

- Fork the repository and submit pull requests.
- Write feature files under `Features/` and corresponding step definitions under `Steps/`.
- Use the CI pipeline to verify changes before merging.

---

## License

Currently, no license is specified. 

---

For questions or support, please open an issue in the repository.﻿

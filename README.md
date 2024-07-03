# Introduction
### Project Overview
**Sqlist.NET** is an advanced Object/Relational Mapping (O/RM) framework designed to streamline the interaction between **.NET** applications and relational databases. Built on the robust foundations of **ADO.NET**, it provides an abstraction API that allows developers to perform database operations without writing raw SQL queries, thereby simplifying the development process. The framework is built to support future integrations with various **RDBMS** providers, ensuring extensibility and flexibility. As of now, **PostgreSQL** is the officially supported database, with plans to integrate more **RDBMS** providers in the future.

### Purpose
The primary purpose of **Sqlist.NET** is to offer a robust and scalable O/RM solution that abstracts the complexities of database interactions. This framework aims to:

- Simplify CRUD (Create, Read, Update, Delete) operations by providing a high-level API.
- Enhance productivity by reducing the amount of boilerplate code needed for database interactions.
- Enable seamless database migrations through a combination of SQL scripts and schema mapping written in YAML.
- Support dynamic database schema changes and versioning to accommodate evolving application requirements.
- Provide a dotnet CLI tool for managing database migrations directly from the project's directory, akin to Entity Framework Core Migration Tools.
- Facilitate the integration of multiple database providers, starting with PostgreSQL, to cater to diverse application needs.
- Ensure high performance and reliability for enterprise-level applications.

By leveraging Sqlist.NET, developers can focus more on business logic and less on the intricacies of database management, leading to faster development cycles and more maintainable codebases.

### Key Features

- **Abstraction API:** Provides a simplified interface for interacting with databases.
- **Provider Support:** Official integration with PostgreSQL, with the potential for more RDBMS integrations.
- **Database Migrations:** Combines SQL scripts and YAML-based schema mappings for robust migration support.
- **Dotnet CLI Tool:** Facilitates migrations and other database operations directly from the project directory.
- **Modular Architecture:** Organized into multiple assemblies, each responsible for specific functionalities, promoting maintainability and scalability.
- **Extensibility:** Designed to accommodate additional RDBMS providers and extend core functionalities as needed.

# Contributing
### Guidelines

- Follow the coding standards specified in the repository.
- Ensure all new code is covered by unit tests.
- Submit pull requests to the dev branch.

### Issues and Bugs

- Report issues using the GitHub Issues tab.
- Provide a detailed description and steps to reproduce the issue.

### Pull Requests

- Fork the repository and create a new branch for your feature or bug fix.
- Ensure your code follows the project’s coding standards.
- Submit a pull request with a detailed description of your changes.

# Architecture
### Overview:
**Sqlist.NET** is structured into multiple assemblies, each responsible for a specific aspect of the framework. The architecture leverages **ADO.NET** to interact with databases at a lower level, ensuring robust and efficient data handling.

### Components

- **Core:** Core O/RM functionalities.
- **Providers:** RDBMS-specific implementations (e.g., PostgreSQL).
- **Migrations:** Tools and libraries for handling database migrations.

### Design Decisions

- Separation of concerns through multiple assemblies.
- Use of YAML for migration roadmaps to simplify schema definition and versioning.

# Deployment
### Environments

- **Development:** Local development setup with a local PostgreSQL instance.
- **Staging:** Pre-production environment for testing.
- **Production:** Live environment with production-grade PostgreSQL.

### Steps

- Configure connection strings and environment variables for each environment.
- Use CI/CD pipelines to automate deployment processes.

# FAQ
### Common Issues

- Connection Issues: Ensure your connection string is correct and the database server is accessible.
- Migration Failures: Check the syntax and format of your YAML roadmap.

### Tips and Tricks

- Use logging to debug issues with migrations and database operations.
- Regularly back up your database before applying migrations.

# License
### Type
Apache License, Version 2.0

### Terms

- Free to use, modify, and distribute.
- Include the original copyright notice in any copies or substantial portions of the software.
- You may not use this file except in compliance with the License.
- Distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
- See the [Apache-2.0 License](http://www.apache.org/licenses/LICENSE-2.0) for the specific language governing permissions and limitations under the License.

# Appendix
### Glossary

- **O/RM:** Object/Relational Mapping
- **RDBMS:** Relational Database Management System
- **YAML:** YAML Ain't Markup Language
- **ADO.NET:** ActiveX Data Objects for .NET, a set of classes that expose data access services for .NET Framework programmers.

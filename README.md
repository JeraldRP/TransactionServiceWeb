# TransactionServiceWeb
A service that can upload transaction data from files of different formats into a database and allows users to query transactions based on specific criteria.



# Transaction Upload Service

## Overview
The **Transaction Upload Service** is a web application that allows users to upload transaction files, view transaction details, and filter transactions based on various criteria such as symbol, date range, order side, and order status.

## Features
- Upload transaction files in Excel format.
- Retrieve transactions by:
  - Symbol
  - Date range
  - Order side (Buy/Sell)
  - Order status (Open/Matched/Cancelled)
- View transactions in a user-friendly table format.

## Technologies Used
- ASP.NET Core
- Entity Framework Core
- SQL Server (or PostgreSQL)
- Razor Pages jQuery/HTML/CSS for front-end
- Bootstrap for styling

## Getting Started

### Prerequisites
- [.NET SDK](https://dotnet.microsoft.com/download) (version X.X.X)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) or [PostgreSQL](https://www.postgresql.org/download/)
- [Visual Studio](https://visualstudio.microsoft.com/) or any preferred IDE

# AD Script Generator

A layered ASP.NET Core application that generates standardized PowerShell `New-ADUser` scripts from structured Excel input.

This project demonstrates enterprise-style architecture, separation of concerns, and automation principles commonly used in IT infrastructure management.

---

## 🎯 Project Purpose

Active Directory user provisioning often involves repetitive, error-prone manual script writing.  
This tool automates the transformation of structured staff data (Excel) into validated and standardized PowerShell scripts.

The focus of this project is not only functionality — but also clean architecture and extensibility.

---

## 🏗 Architecture Overview

The solution follows a layered design to ensure scalability and reusability:
```
AdScriptGenerator
│
├── AdScript.Core
│ ├── Models
│ ├── Services
│ ├── Validation
│
├── AdScript.Web
│ ├── Razor Pages
│
└── (Planned)
└── AdScript.Blazor
```
---

### 🔹 Core Layer
Responsible for all business logic:

- Excel parsing
- Data sanitisation
- Naming standard enforcement
- Validation rules
- PowerShell script generation

The Core layer has no dependency on ASP.NET, enabling reuse across:
- Web applications
- Desktop applications
- Future Blazor implementation

---

### 🔹 Web Layer
Handles:

- File upload
- Column mapping
- Script preview
- Script download

The Web layer acts purely as a UI wrapper over the Core logic.

---

## ⚙ Technology Stack

- .NET 8
- ASP.NET Core Razor Pages
- Clean architecture principles
- Excel processing (ClosedXML / EPPlus)
- PowerShell script generation

---

## 📐 Engineering Principles Applied

- Separation of Concerns
- Layered Architecture
- Reusable Core Logic
- Validation Before Execution
- Enterprise Naming Standards
- Future-proof design for UI refactor (Blazor)

---

## 📋 Naming Standard Implementation

### SamAccountName
- Lowercase
- Special characters removed
- Max 20 characters (legacy NetBIOS constraint)
- Deterministic generation

### UserPrincipalName
- Email-style format
- Based on sanitized username
- Configurable domain suffix

### OU Path
- Multi-level dynamic OU construction
- Based on structured organizational data

### Validation Rules
- Prevent conflicting password flags
- Input sanitation before script generation
- Defensive rule enforcement


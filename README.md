# MyBookShop

**MyBookShop** is an online book store built with **.NET 10**, designed to manage books, users, roles, and payments. This project is a great example for learning **Entity Framework Core**, and **JWT Authentication**.

---

## ⚡ Features

- Book management: add, edit, and delete books  
- User and role management (Admin / User)  
- **JWT Authentication** for secure API access  
- **Zarinpal Payment Gateway** integration for online payments  
- Seed data for quick setup and testing  

---

## 🛠 Technologies

- **Backend:** .NET 10, C#  
- **Database:** SQL Server (EF Core)  
- **Authentication:** ASP.NET Core Identity + JWT  
- **Payment Gateway:** Zarinpal Sandbox  
- **Tools:** Visual Studio 2026, Git  

---

## 🔒 Secrets

This project uses sensitive values like JWT Secret and Zarinpal MerchantId.  

Set them using **User Secrets** (for development) or **Environment Variables** (for production):

```bash
dotnet user-secrets set "Jwt:Secret" "YOUR_SECRET_KEY"
dotnet user-secrets set "Zarinpal:MerchantId" "YOUR_MERCHANT_ID"
```

---

## 📌 Notes

- This project is created for learning purposes
- Sensitive files are excluded via .gitignore
- Make sure to set sensitive secrets (JWT, MerchantId)

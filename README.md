# 🔐 Login / Signup / Real-Time Chat Application

A full-stack web application built using **Angular (Material UI)** and **.NET**, implementing secure authentication, role-based authorization, real-time messaging, and analytics dashboards.

---

## 📌 Project Overview

This application allows users to:

* Register and request access
* Login securely using JWT authentication
* Communicate in real-time via a notice board/chat system
* View role-based dashboards with analytics
* Experience secure session management and encryption

The system includes **Admin approval workflows**, **real-time updates using SignalR**, and **data security using RSA encryption + hashing**.

---

## 🏗️ Tech Stack

### Frontend

* Angular
* Angular Material UI
* TypeScript

### Backend

* .NET (ASP.NET Core Web API)
* SignalR (Real-time communication)
* JWT Authentication

### Databases

* Apache CouchDB → User Data
* PostgreSQL → Messages & Analytics

---

## 🔑 Features

### 🔐 Authentication & Authorization

* JWT-based authentication
* Role-based access control (Admin/User)
* Protected routes (No direct URL access without login)
* Single session login (No multiple tabs/devices)

---

### 📝 User Registration

* Users can register with:

  * First Name, Last Name
  * Email
  * Password (with confirmation)
* Registration request sent to Admin
* Admin approves and assigns role (Admin/User)

---

### 👨‍💼 Admin Controls

* Approve or reject user registrations
* Assign roles to users
* Access extended dashboard features

---

### 💬 Real-Time Chat / Notice Board

* Send messages to specific roles/groups
* View:

  * Sent messages (right side)
  * Received messages (left side)
* Message details include:

  * Sender
  * Receiver
  * Timestamp
* Real-time updates using SignalR
* Messages stored in PostgreSQL

---

### 📊 Dashboard & Analytics

* Visual analytics powered by PostgreSQL
* Includes:

  * 📈 Bar & Line Charts
  * 🥧 Pie Charts
  * 🌍 Geo-based Views
  * 🔥 Heatmaps / Activity charts
* Auto-refresh on new data updates

---

### 🔒 Data Security

* RSA Encryption for stored data
* Password hashing
* Secure communication via JWT tokens

---

### ⏱️ Session Management

* Session timeout:

  * Admin → Tab A (20 seconds)
  * User → Tab F (20 seconds)
* Prevent multiple logins across tabs/browsers

---

### 🚪 Logout

* Clears:

  * Cache
  * Session data
  * Credentials
* Disconnects SignalR sessions

---

## ⚙️ Project Structure (High Level)

```
Frontend (Angular)
│
├── Components (Login, Signup, Dashboard, Chat)
├── Services (Auth, API, SignalR)
├── Guards (Route Protection)
│
Backend (.NET)
│
├── Controllers
├── Services
├── Middleware (JWT, Authorization)
├── SignalR Hub
│
Databases
├── CouchDB (User Data)
└── PostgreSQL (Messages + Analytics)
```

---

## 🚀 How It Works (Flow)

1. User registers → request sent to Admin
2. Admin approves → role assigned
3. User logs in → JWT token generated
4. Middleware validates token
5. User redirected to role-based dashboard
6. Chat messages handled via SignalR
7. Data stored securely in databases

---

## 🧪 Key Concepts Implemented

* JWT Authentication (Access Token)
* Role-Based Authorization
* Middleware & Interceptors
* SignalR for real-time communication
* RSA Encryption + Hashing
* Session Management
* RESTful APIs

---

## 📷 Future Improvements

* Refresh Token implementation
* Mobile responsiveness improvements
* Notification system
* File sharing in chat
* Dark mode UI

---

## 📄 Reference

This project is implemented based on the following requirement specification:


---

## 👨‍💻 Author

**Abhinav Garg**

---

## ⭐ If you like this project

Give it a ⭐ on GitHub and share it!

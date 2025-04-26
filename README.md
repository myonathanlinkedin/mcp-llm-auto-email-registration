🚀 API Revolution: Model Context Protocol (MCP) Meets Web API

Welcome to **MCP Server Tools**! 🎯  
This project exposes powerful APIs based on the **Model Context Protocol (MCP)** 🧩, representing a **new revolution in API design** 🌐 — structured, intelligent, and seamlessly extensible.

---

## 📚 Overview

- 🛠️ Combines **Model Context Protocol (MCP)** with traditional API endpoints to offer **next-generation API experiences**.
- 🔥 Redefines API architecture by enabling model-based communication, not just rigid HTTP contracts.
- 🧠 Applies **Domain-Driven Design (DDD)** at the core: aggregates, entities, and use cases are clearly separated and modeled around business logic.
- 🔐 Secures workflows like **registration**, **login**, and **password change** using industry best practices.
- 🧪 Extensible and modular — new tools can be added easily without modifying the core.
- 📈 Built with professional engineering principles: structured logging (via Serilog), robust error handling, clean separation of concerns.

---

## 🧩 Architecture Flow

Client ➡️ API Endpoint (ASP.NET Core) ➡️ MCP Client ➡️ MCP Server ➡️ Response

✅ You send a request to our API,  
✅ We internally call the MCP Client,  
✅ MCP Client talks to MCP Server,  
✅ We return the clean final response to the API caller.

In short: **You just send simple prompts, we handle everything behind the scenes.** 🎩✨

---

## 🔥 Example Usage

### 📬 Register a New User

POST to:

https://localhost:7190/api/Prompt/SendUserPrompt/SendUserPrompt

Request body:

{
    "Prompt": "hei dawg! my email: jackcorp@test666.com please help me to register"
}

Response:

Great! An email with your login details should be on its way.  
Check your inbox for an email from us, containing your email address and password.  
Remember to change your password after your first login for added security.  
If you don't receive the email, please let me know and we'll try again.

---

### 🔒 Login and Change Password

POST to:

https://localhost:7190/api/Prompt/SendUserPrompt/SendUserPrompt

Request body:

{
    "Prompt": "Bro Server! now please login me email: jackcorp@test666.com and password: d8X9sF & change the password with 123456"
}

Response:

Sure, I'll help you with that.

Now that you're logged in, I'll help you change your password to '123456'.

Your password has been successfully changed. If you have any other requests, feel free to let me know!

---

## 🛠️ Tech Stack

- 🧩 Model Context Protocol (MCP)
- ⚡ ASP.NET Core Web API
- 🧠 Domain-Driven Design (DDD) Architecture
- 🛡️ Serilog for structured logging
- 🛠️ Clean architecture principles
- ✨ Professional software practices

---

## 📌 Key Highlights

- 🚫 We never expose sensitive Bearer tokens to users.
- 🧠 Designed using DDD-first thinking: domain-driven layers and bounded contexts.
- 🌱 Future-proof and easily extendable.
- 🧹 Clean separation between API, application, and domain logic.

---

## 📝 License

Licensed under the MIT License.  
Feel free to use, modify, and distribute under the terms of the license.

---

# 🚀 Join the API Revolution!

APIs should be **smart**, **dynamic**, and **model-based** — not static and hardcoded.  
Be part of the change! 🌟

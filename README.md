# .NET Garbage Collection PoC with Scoped Services

This project demonstrates a **Proof of Concept (PoC)** for observing and validating **Garbage Collection (GC)** behavior in a .NET Minimal API application using `Scoped` services and `WeakReference` tracking in unit tests.

## 🧠 Objectives

- Understand how .NET GC handles `Scoped` services in web applications
- Prove that a service instantiated per-request will be eligible for GC after request completion
- Simulate a memory leak and verify that GC does not collect retained instances
- Compare memory usage before and after GC collection

---

## 🔧 Tech Stack

- [.NET 9](https://dotnet.microsoft.com/en-us/download)
- Minimal API
- `xUnit` for unit testing
- `Microsoft.AspNetCore.Mvc.Testing` for in-memory web testing

---

## 📁 Project Structure

```text
DotNetGarbage/
├── Program.cs                  # Minimal API setup
├── Services/
│   ├── IHeavyService.cs       # Interface for memory-consuming service
│   └── HeavyService.cs        # Implementation that allocates 50MB
│
DotNetGarbage.Tests/
├── GarbageCollectionApiTests.cs # xUnit tests to verify GC behavior



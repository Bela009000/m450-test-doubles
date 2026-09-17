# Dependency injection and testability

Dependency injection (DI) means that a class receives the objects it depends on from the outside instead of creating them itself. Those objects are called **dependencies**.

This folder compares two versions of a checkout service:

- [`BadExample.cs`](BadExample.cs) creates its own email sender.
- [`GoodExample.cs`](GoodExample.cs) receives a receipt sender through its constructor.

Constructor injection is usually the clearest form of DI because a class cannot be created without its required dependencies.

Build both examples from the repository root with:

```bash
dotnet build dependency-injection/DependencyInjectionExamples.csproj
```

## The problem without dependency injection

The bad example contains this relationship:

```text
CheckoutService -> creates EmailReceiptSender -> performs external work
```

`CheckoutService` controls both the checkout behaviour and the construction of its dependency. A unit test cannot replace `EmailReceiptSender` with a harmless test double. The test may become slow, send a real email, require network configuration, or fail for reasons unrelated to checkout logic.

## The design with dependency injection

The good example changes the relationship:

```text
Production code -> injects EmailReceiptSender -> CheckoutService
Test code       -> injects RecordingReceiptSender -> CheckoutService
```

`CheckoutService` depends on the `IReceiptSender` abstraction. Its constructor accepts any implementation of that interface. Production code supplies the real sender, while a unit test supplies a small in-memory test double.

The important line is:

```csharp
var service = new CheckoutService(receiptSender);
```

The caller creates the dependency and injects it into the service. No DI container is required to use this pattern; a container only automates object creation in larger applications.

## Why DI is key for writing unit tests

Dependency injection helps tests remain:

- **Isolated:** the class is tested without a database, email server, file system, clock, or web API.
- **Deterministic:** the test controls what each dependency returns, so the same input produces the same result every time.
- **Fast:** in-memory test doubles avoid network and disk operations.
- **Safe:** tests do not send real messages or modify production data.
- **Focused:** a failure points to the class under test instead of an external service.
- **Easy to arrange:** each test can inject the exact behaviour required for its scenario.

DI does not make a test good automatically. It makes isolation possible. Tests should still verify public behaviour and follow the [Arrange, Act, Assert pattern](../AAA_PATTERN.md).

## How the good example can be tested

Use `RecordingReceiptSender` from `GoodExample.cs` as a test double:

1. **Arrange:** create a `RecordingReceiptSender` and inject it into `CheckoutService`.
2. **Act:** call `CompleteCheckout` with a known email address and total.
3. **Assert:** verify that the recording sender received the expected address and total.

You can also write a test that passes an invalid email address and verifies that no receipt was recorded. The unit test never needs to contact a real email system.

## Exercise

1. Read both examples and locate where the dependency is created.
2. Explain why replacing the sender is difficult in the bad example.
3. Create a unit test for the good example using `RecordingReceiptSender`.
4. Add another dependency, such as an `ITimeProvider`, and inject it through the constructor.
5. Discuss which dependencies are worth abstracting. Simple value objects and pure calculations usually do not need interfaces.

The goal is not to create an interface for every class. Introduce a boundary when a dependency has side effects, is slow or unpredictable, or needs different implementations.
